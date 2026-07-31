using System.Net;

namespace BabaPlay.Web.Services.Http;

internal static class FinancialApiErrorMessages
{
    internal static string FromStatusCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized =>
            "Sua sessão expirou ou não está autenticada. Faça login novamente.",
        HttpStatusCode.Forbidden =>
            "Você não tem permissão para acessar os dados financeiros desta associação.",
        _ => "Não foi possível carregar os dados financeiros. Verifique sua conexão ou permissões.",
    };
}
