//using Microsoft.Extensions.DependencyInjection;
//using SoftwarePioniere.Builder;

//namespace SoftwarePioniere.Hosting
//{
//    public static class SopiBuilderHealthChecksExtensions
//    {

//        public static ISopiBuilder AddHealthChecks(this ISopiBuilder builder)
//        {
//            var services = builder.Services;

//            builder.AddFeature("HealthChecksBuilder", services.AddHealthChecks());     

//            return builder;
//        }

//        public static IHealthChecksBuilder GetHealthChecksBuilder(this ISopiBuilder builder)
//        {
//            return builder.GetFeature<IHealthChecksBuilder>("HealthChecksBuilder");
//        }

//    }
//}