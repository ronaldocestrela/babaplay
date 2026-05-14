#!/usr/bin/env bash
# run-tests-local.sh - Run backend and frontend tests locally.
# Usage: ./scripts/run-tests-local.sh [backend|frontend|all]

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKEND_DIR="$ROOT/Backend"
WEB_DIR="$ROOT/web"
TARGET="${1:-all}"

log() {
  printf "\n[run-tests] %s\n" "$1"
}

fail() {
  printf "\n[run-tests][error] %s\n" "$1" >&2
  exit 1
}

run_backend_tests() {
  log "Running backend tests (.NET)"

  if ! command -v dotnet >/dev/null 2>&1; then
    fail "dotnet not found in PATH"
  fi

  (
    cd "$BACKEND_DIR"
    dotnet test BabaPlay.slnx --configuration Release --verbosity minimal
  )
}

run_frontend_tests() {
  log "Running frontend tests (Vitest)"

  if ! command -v yarn >/dev/null 2>&1; then
    fail "yarn not found in PATH"
  fi

  (
    cd "$WEB_DIR"
    yarn test:run
  )
}

case "$TARGET" in
  backend)
    run_backend_tests
    ;;
  frontend)
    run_frontend_tests
    ;;
  all)
    run_backend_tests
    run_frontend_tests
    ;;
  *)
    fail "invalid target '$TARGET'. Use: backend | frontend | all"
    ;;
esac

log "Done"
