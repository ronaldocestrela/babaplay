#!/usr/bin/env bash
# ci-local.sh — Executa a mesma esteira do CI (GitHub Actions) localmente.
# Uso: ./scripts/ci-local.sh [backend]

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKEND_DIR="$ROOT/Backend"

# ─── Cores ───────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; RESET='\033[0m'

step()  { echo -e "\n${CYAN}${BOLD}▶ $*${RESET}"; }
ok()    { echo -e "${GREEN}✔ $*${RESET}"; }
fail()  { echo -e "${RED}✖ $*${RESET}"; exit 1; }
warn()  { echo -e "${YELLOW}⚠ $*${RESET}"; }

# ─── Backend ─────────────────────────────────────────────────────────────────
run_backend() {
  echo -e "\n${BOLD}━━━ BACKEND ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"

  step "Verificando .NET 10..."
  dotnet --version | grep -q "^10\." || fail "Requer .NET 10. Instale em https://dotnet.microsoft.com/download"
  ok "$(dotnet --version)"

  step "Restaurando dependências..."
  (cd "$BACKEND_DIR" && dotnet restore BabaPlay.slnx --verbosity quiet)
  ok "Restore concluído"

  step "Build (Release)..."
  (cd "$BACKEND_DIR" && dotnet build BabaPlay.slnx --no-restore --configuration Release --verbosity quiet)
  ok "Build concluído"

  step "Publicando Blazor WebAssembly..."
  (cd "$BACKEND_DIR" && dotnet publish src/BabaPlay.Web/BabaPlay.Web.csproj -c Release -o ./publish-blazor --verbosity quiet)
  ok "Publish Blazor concluído"

  step "Executando testes com cobertura..."
  rm -rf "$BACKEND_DIR/coverage"
  (cd "$BACKEND_DIR" && dotnet test BabaPlay.slnx \
    --no-build \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory ./coverage \
    --logger "trx;LogFileName=test-results.trx" \
    --verbosity normal)
  ok "Testes concluídos"

  step "Verificando threshold de cobertura (>= 80%)..."
  if ! command -v reportgenerator &>/dev/null; then
    warn "reportgenerator não encontrado — instalando..."
    dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources
  fi
  (cd "$BACKEND_DIR" && reportgenerator \
    -reports:"./coverage/**/coverage.cobertura.xml" \
    -targetdir:"./coverage/report" \
    -reporttypes:"TextSummary" \
    -minimumcoverageThresholds:"linecoverage=80")
  ok "Cobertura OK"

  echo -e "\n  Relatório: ${CYAN}file://$BACKEND_DIR/coverage/report/Summary.txt${RESET}"
}

# ─── Resultado final ──────────────────────────────────────────────────────────
print_summary() {
  local status=$1
  echo ""
  if [[ $status -eq 0 ]]; then
    echo -e "${GREEN}${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
    echo -e "${GREEN}${BOLD}  ✔  CI local passou com sucesso!${RESET}"
    echo -e "${GREEN}${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
  else
    echo -e "${RED}${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
    echo -e "${RED}${BOLD}  ✖  CI local falhou.${RESET}"
    echo -e "${RED}${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
  fi
}

# ─── Entrypoint ───────────────────────────────────────────────────────────────
TARGET="${1:-backend}"

trap 'print_summary $?' EXIT

case "$TARGET" in
  backend|all) run_backend ;;
  *)
    echo "Uso: $0 [backend|all]"
    exit 1
    ;;
esac

