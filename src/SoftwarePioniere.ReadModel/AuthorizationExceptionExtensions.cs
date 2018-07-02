//using System.Collections.Generic;
//using System.Net;

//namespace SoftwarePioniere.ReadModel
//{
//    public static class AuthorizationExceptionExtensions
//    {
//        public static PagedResults<T> ToEmptyPagedResults<T>(this AuthorizationException authorizationException)
//        {
//            return new PagedResults<T>
//            {
//                Results = new List<T>(),
//                Error = authorizationException.Message,
//                HttpStatusCode = HttpStatusCode.Forbidden
//            };
//        }
//    }
//}