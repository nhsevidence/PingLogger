using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;

namespace PingLogger
{
    public class RequestEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestEnricher(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var request = _httpContextAccessor.HttpContext.Request;

                logEvent.AddPropertyIfAbsent(
                    propertyFactory.CreateProperty("RequestPath", request.Path, false));

                logEvent.AddPropertyIfAbsent(
                    propertyFactory.CreateProperty("RequestMethod", request.Method, false));

                logEvent.AddPropertyIfAbsent(
                    propertyFactory.CreateProperty("RequestQueryString", request.QueryString.ToString(), false));

                logEvent.AddPropertyIfAbsent(
                    propertyFactory.CreateProperty("RequestHost", request.Host.ToString(), false));

                string headers = string.Empty;
                foreach (var key in request.Headers.Keys)
                    headers += key + "=" + request.Headers[key] + Environment.NewLine;

                logEvent.AddPropertyIfAbsent(
                    propertyFactory.CreateProperty("RequestHeaders", headers, false));

            }
        }
    }
}
