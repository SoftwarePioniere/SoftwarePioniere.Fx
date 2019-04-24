//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

//namespace SoftwarePioniere.Messaging
//{
//    public interface ITelemetryAdapter
//    {
//        Task<T> RunWithResultAsync<T>(string operationName
//            , Func<IDictionary<string, string>, Task<T>> runx
//            , IDictionary<string, string> parentState
//            , ILogger logger
//        );

//        Task RunDependencyAsync(string operationName, string type
//            , Func<IDictionary<string, string>, Task> runx
//            , IDictionary<string, string> parentState
//            , ILogger logger
//            );
//    }
//}