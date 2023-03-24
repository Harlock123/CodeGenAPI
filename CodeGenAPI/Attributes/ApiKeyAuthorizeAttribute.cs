using System;
using CodeGenAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CodeGenAPI.Attributes
{
    ///<Summary>
    /// The ApiKeyAuthorizeAttribute class is used to validate the API Key
    ///</Summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ApiKeyAuthorizeAttribute : TypeFilterAttribute
	{
        ///<Summary>
        /// The ApiKeyAuthorizeAttribute constructor
        ///</Summary>
        public ApiKeyAuthorizeAttribute() : base(typeof(ApiKeyAuthorizeAsyncFilter))
		{
		}
	}
}
