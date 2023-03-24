using System;
using System.Threading.Tasks;

namespace CodeGenAPI
{
    ///<Summary>
    /// The KeyAuthorize class is used to validate the API Key
    ///</Summary>
    public class KeyAuthorize: IApiKeyService
    {
                
        public Task<bool> IsAuthorized(string clientId, string apiKey)
        {
            //throw new NotImplementedException();
            // Here we accept them but we should test each one here and validate

            return Task.FromResult(true); ;
        }
    }
}
