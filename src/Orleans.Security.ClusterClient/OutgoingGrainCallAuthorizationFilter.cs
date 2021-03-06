﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Security.AccessToken;
using Orleans.Security.Authorization;

namespace Orleans.Security.ClusterClient
{
    public class OutgoingGrainCallAuthorizationFilter : GrainCallAuthorizationFilterBase, IOutgoingGrainCallFilter
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        private readonly OAuth2EndpointInfo _oAuth2EndpointInfo;

        public OutgoingGrainCallAuthorizationFilter(IAccessTokenProvider accessTokenProvider,
            IAccessTokenValidator accessTokenValidator,
            OAuth2EndpointInfo oAuth2EndpointInfo, IAuthorizeHandler authorizeHandler,
            ILoggerFactory loggerFactory) : base(accessTokenValidator, authorizeHandler)
        {
            _accessTokenProvider = accessTokenProvider;
            _oAuth2EndpointInfo = oAuth2EndpointInfo;

            Logger = loggerFactory.CreateLogger<OutgoingGrainCallAuthorizationFilter>();
        }

        public async Task Invoke(IOutgoingGrainCallContext context)
        {
            if (AuthenticationChallenge(context))
            {
                var accessToken = _accessTokenProvider.RetrieveToken();

                await AuthorizeAsync(context, accessToken, _oAuth2EndpointInfo);

                var grainType = context.Grain.GetType();

                Logger.LogTrace(LoggingEvents.OutgoingGrainCallAuthorizationPassed,
                    $"{LoggingEvents.OutgoingGrainCallAuthorizationPassed.Name} Type of Grain: {grainType.Name} " +
                    $"Method Name: {context.InterfaceMethod.Name} ");

                RequestContext.Set(ConfigConstants.AccessTokenKey, accessToken);
                RequestContext.Set(ConfigConstants.OAuth2EndpointInfoKey, _oAuth2EndpointInfo);
            }

            await context.Invoke();
        }
    }
}
