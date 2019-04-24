//using System;
//using System.Collections.Generic;

//namespace SoftwarePioniere.Messaging
//{
//    public static class TelemetryAdapterExtensions
//    {
//        public static Dictionary<string, object> CreateLoggerScope(this IDictionary<string, string> state)
//        {
//            var dict = new Dictionary<string, object>();
//            foreach (var key in state.Keys)
//            {
//                dict.Add(key, state[key]);
//            }
//            return dict;
//        }


//        public static string GetInnerExceptionMessage(this Exception ex)
//        {
//            if (ex.InnerException != null)
//            {
//                return ex.InnerException.GetInnerExceptionMessage();
//            }

//            return ex.Message;
//        }

//        public static IDictionary<string, string> AppendCommand(this IDictionary<string, string> state, ICommand cmd)
//        {
//            var t = cmd.GetType();

//            state.AddProperty("CommandType", t.FullName);

//            state.AddProperty("CommandId", cmd.Id.ToString());
//            state.AddProperty("CommandObjectId", cmd.ObjectId);
//            state.AddProperty("CommandObjectType", cmd.ObjectType);

//            return state;
//        }

         
//        public static IDictionary<string, string> CreateState(this ICommand cmd)
//        {
//            return new Dictionary<string, string>().AppendCommand(cmd);
//        }

//        public static string GetOperationId(this IDictionary<string, string> dict)
//        {
//            if (!dict.ContainsKey("OperationId"))
//                return string.Empty;

//            return dict.GetProperty("OperationId");
//        }

//        public static string GetParentRequestId(this IDictionary<string, string> dict)
//        {
//            if (!dict.ContainsKey("ParentRequestId"))
//                return string.Empty;

//            return dict.GetProperty("ParentRequestId");
//        }

//        public static IDictionary<string, string> SetOperationId(this IDictionary<string, string> dict, string value)
//        {
//            return dict.AddProperty("OperationId", value);
//        }

//        public static IDictionary<string, string> SetParentRequestId(this IDictionary<string, string> dict,
//            string value)
//        {
//            return dict.AddProperty("ParentRequestId", value);
//        }

//    }
//}
