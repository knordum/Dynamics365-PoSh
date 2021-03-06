﻿using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamics365_PoSh.Helpers
{
    public class OAuthHelper
    {
        private static string _clientId = "2ad88395-b77d-4561-9441-d0e40824f9bc";
        private static string _redirectUrl = "app://5d3e90d6-aa8e-48a8-8f2c-58b45cc67315/";

        public Uri Endpoint = null;
        private string _resource = null;
        private string _authority = null;
        private AuthenticationContext _authContext = null;
        private AuthenticationResult _authResult = null;

        public OAuthHelper(Uri endpoint, string clientId = null, string redirectUrl = null)
        {
            Endpoint = endpoint;
            _clientId = clientId ?? _clientId;
            _redirectUrl = redirectUrl ?? _redirectUrl;
        }

        public string Authority
        {
            get
            {
                if (_authority == null)
                {
                    DiscoverAuthority(Endpoint);
                }
                return _authority;
            }
        }

        public AuthenticationContext AuthContext
        {
            get
            {
                if (_authContext == null)
                {
                    _authContext = new AuthenticationContext(Authority, false);
                }
                return _authContext;
            }
        }

        public AuthenticationResult AuthResult
        {
            get
            {
                Authorize();
                return _authResult;
            }
        }

        public HttpMessageHandler Handler
        {
            get
            {
                return new OAuthMessageHandler(this, new HttpClientHandler());
            }
        }

        private void DiscoverAuthority(Uri discoveryUrl)
        {
            try
            {
                Task.Run(async () =>
                {
                    var ap = await AuthenticationParameters.CreateFromResourceUrlAsync(discoveryUrl);
                    _resource = ap.Resource;
                    _authority = ap.Authority;
                }).Wait();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void Authorize()
        {
            if (_authResult == null || _authResult.ExpiresOn.AddMinutes(-30) < DateTime.Now)
            {
                Task.Run(async () =>
                {
                    _authResult = await AuthContext.AcquireTokenAsync(_resource, _clientId, new Uri(_redirectUrl),
                    new PlatformParameters(PromptBehavior.Always));
                }).Wait();
            }
        }

        class OAuthMessageHandler : DelegatingHandler
        {
            OAuthHelper _auth = null;
            public OAuthMessageHandler(OAuthHelper auth, HttpMessageHandler innerHandler) : base(innerHandler)
            {
                _auth = auth;
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AuthResult.AccessToken);
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}