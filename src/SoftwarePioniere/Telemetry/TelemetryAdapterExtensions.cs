using System;
using System.Collections.Generic;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Telemetry
{
    public static class TelemetryAdapterExtensions
    {
        public static Dictionary<string, object> CreateLoggerScope(this Dictionary<string, string> state)
        {
            var dict = new Dictionary<string,object>();

            if (state != null)
            {
                foreach (var keyValuePair in state)
                {
                    dict.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return dict;
        }


        public static string GetInnerExceptionMessage(this Exception ex)
        {
            if (ex.InnerException != null)
            {
                return ex.InnerException.GetInnerExceptionMessage();
            }

            return ex.Message;
        }

        public static Dictionary<string, string> AppendCommand(this Dictionary<string, string> state, ICommand cmd)
        {
            var t = cmd.GetType();

            state.AddProperty("CommandType", t.FullName);

            state.AddProperty("CommandId", cmd.Id.ToString());
            state.AddProperty("CommandObjectId", cmd.ObjectId);
            state.AddProperty("CommandObjectType", cmd.ObjectType);

            return state;
        }


        public static Dictionary<string, string> CreateState(this ICommand cmd)
        {
            return new Dictionary<string, string>().AppendCommand(cmd);
        }

        public static string GetOperationId(this Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey("OperationId"))
                return string.Empty;

            return dict.GetProperty("OperationId");
        }

        public static string GetParentRequestId(this Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey("ParentRequestId"))
                return string.Empty;

            return dict.GetProperty("ParentRequestId");
        }

        public static Dictionary<string, string> SetOperationId(this Dictionary<string, string> dict, string value)
        {
            return dict.AddProperty("OperationId", value);
        }

        public static Dictionary<string, string> SetParentRequestId(this Dictionary<string, string> dict,
            string value)
        {
            return dict.AddProperty("ParentRequestId", value);
        }

    }
}
