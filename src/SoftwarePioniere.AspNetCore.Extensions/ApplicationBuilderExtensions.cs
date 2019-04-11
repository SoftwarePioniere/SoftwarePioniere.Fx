using System;
using Microsoft.AspNetCore.Builder;

namespace SoftwarePioniere.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder When(
            this IApplicationBuilder builder,
            bool predicate,
            Func<IApplicationBuilder> compose) => predicate ? compose() : builder;
    }
}
