﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using NUnit.Framework;
using Orleans.Security.AccessToken;

namespace Orleans.Security.IntegrationTests.Extensions
{
    internal class FakeAccessTokenValidator : IAccessTokenValidator
    {
        public static LoggedInUser LoggedInUser { private get; set; }

        public Task<AccessTokenValidationResult> Validate(string accessToken,
            OAuth2EndpointInfo oAuth2EndpointInfo)
        {
            var userName = TestContext.CurrentContext.Test.FullName;
            var loggedInUser = (int) LoggedInUser;

            if (!AuthorizationTestConfig.Claims.ContainsKey(loggedInUser))
            {
                throw new InvalidOperationException($"Invalid LoggedInUser value for {userName}.");
            }

            // ReSharper disable once SuggestVarOrType_Elsewhere
            IEnumerable<Claim> claims = AuthorizationTestConfig.Claims[loggedInUser];

            return Task.FromResult(AccessTokenValidationResult.CreateSuccess(AccessTokenType.Jwt,
                claims));
        }
    }
}
