namespace SoftwarePioniere.Extensions.AspNetCore
{
    public interface ISwaggerClientOptions
    {
        string SwaggerClientId { get; } 
        string SwaggerClientSecret { get; }
        string SwaggerAuthorizationUrl { get; }
        string SwaggerResource { get; }
    }
}
