namespace SoftwarePioniere.Clients.AzureAd
{
    public class AzureAdClientOptions
    {
        public string ClientSecret { get; set; }

        public string ClientId { get; set; }

        public string TenantId { get; set; }

        public string Authority => $"https://login.microsoftonline.com/{TenantId}";
    }
}
