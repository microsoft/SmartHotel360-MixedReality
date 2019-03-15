using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace SmartHotel.MixedReality.Api.Auth
{
    public class AuthorizationFilterAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _secureApiKey;
        private bool _authEnabled;

        public AuthorizationFilterAttribute(IOptions<AuthorizationSettings> authorizationOptions)
        {
            _secureApiKey = authorizationOptions.Value.Apikey;
            _authEnabled = authorizationOptions.Value.AuthEnabled;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!_authEnabled)
                return;
            StringValues apiKey = context.HttpContext.Request.Headers["X-API-KEY"];

            if (apiKey.Any())
            {
                if (apiKey[0] != _secureApiKey)
                {
                    context.Result = new UnauthorizedResult();
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}