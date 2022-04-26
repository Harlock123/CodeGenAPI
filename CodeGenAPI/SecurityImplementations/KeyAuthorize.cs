using System;
using System.Threading.Tasks;

namespace CodeGenAPI
{
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
