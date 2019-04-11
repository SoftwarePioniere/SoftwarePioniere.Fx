using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace SoftwarePioniere.Messaging.Services.ApplicationInsights
{
    public static class TelemetryHelpers
    {
        public static IDictionary<string, string> AppendRequest(this IDictionary<string, string> state,
            HttpContext httpContext)
        {
            var requestTelemetry = httpContext.Features.Get<RequestTelemetry>();

            if (requestTelemetry != null)
            {
                state.SetParentRequestId(requestTelemetry.Id);
                state.SetOperationId(GetOperationId(requestTelemetry.Id));
            }

            state.SetTraceIdentifier(httpContext.TraceIdentifier);

            return state;
        }

        public static IDictionary<string, string> CreateState(this HttpContext httpContext)
        {
            return new Dictionary<string, string>().AppendRequest(httpContext);
        }

        public static string GetOperationId(string id)
        {
            // Returns the root ID from the '|' to the first '.' if any.
            var rootEnd = id.IndexOf('.');
            if (rootEnd < 0)
            {
                rootEnd = id.Length;
            }

            var rootStart = id[0] == '|' ? 1 : 0;
            return id.Substring(rootStart, rootEnd - rootStart);
        }

    }
}