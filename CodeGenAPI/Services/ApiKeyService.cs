using System;
using System.Threading.Tasks;

namespace CodeGenAPI
{
    public interface IApiKeyService
    {
        Task<bool> IsAuthorized(string clientId, string apiKey);
    }
}
