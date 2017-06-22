﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.WebServiceClient;
using System;
using System.Net;

namespace Dynamics365_PoSh.Helpers
{
    public class CrmConnection
    {
        /// <summary>
        /// Generates a new OrganizationWebProxyClient based on a server URL and authentication cookies
        /// </summary>
        /// <param name="serverUrl">Ex: https://org.crm4.dynamics.com</param>
        /// <param name="cookies">Collection of authentication cookies</param>
        /// <param name="useStrongTypes">Use early bound types</param>
        /// <returns>Authenticated OrganizationWebProxyClient</returns>
        public static IOrganizationService GetConnection(string serverUrl, CookieCollection cookies, bool useStrongTypes = false)
        {
            var cookieContainer = new CookieContainer();
            foreach (Cookie cookie in cookies)
            {
                cookieContainer.Add(cookie);
            }
            var serviceUrl = new Uri($"{serverUrl}/XRMServices/2011/Organization.svc/web");
            var client = new OrganizationWebProxyClient(serviceUrl, useStrongTypes);
            var cookieBehavior = new CookieBehavior(cookieContainer);
            client.Endpoint.EndpointBehaviors.Add(cookieBehavior);

            return client;
        }
    }
}