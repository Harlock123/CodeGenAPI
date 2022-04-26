using System;
using CodeGenAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CodeGenAPI.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ApiKeyAuthorizeAttribute : TypeFilterAttribute
	{
		public ApiKeyAuthorizeAttribute() : base(typeof(ApiKeyAuthorizeAsyncFilter))
		{
		}
	}
}
