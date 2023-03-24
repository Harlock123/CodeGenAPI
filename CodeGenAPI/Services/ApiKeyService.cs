using System;
using System.Threading.Tasks;

namespace CodeGenAPI
{
    ///<Summary>
    /// The IApiKeyService interface is used to validate the API Key
    ///</Summary>
    public interface IApiKeyService
    {
        Task<bool> IsAuthorized(string clientId, string apiKey);
    }
}
