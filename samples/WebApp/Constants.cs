namespace WebApp
{
    public static class Constants
    {
        public const string ApiTitle = "SoPi-SAMPLES";

        /// <summary>
        /// API Key
        /// </summary>
        public const string ApiKey = "test";

        /// <summary>
        /// Bais Router der API
        /// </summary>
        public const string ApiBaseRoute = "sopi/api/test";

        public const string NotificationsBaseRoute = "/" + ApiBaseRoute + "/notifications";

        public const string NotificationsBaseRouteAuth = "/" + ApiBaseRoute + "/notifications/auth";

        /// <summary>
        /// Basis Route des Command
        /// </summary>
        public const string CommandBaseRoute = ApiBaseRoute + "/command";

        /// <summary>
        /// Basis Route des Query
        /// </summary>
        public const string QueryBaseRoute = ApiBaseRoute + "/query";


        /// <summary>
        /// Basis Route des Projection
        /// </summary>
        public const string ProjectionBaseRoute = ApiBaseRoute + "/projection";
    }
}