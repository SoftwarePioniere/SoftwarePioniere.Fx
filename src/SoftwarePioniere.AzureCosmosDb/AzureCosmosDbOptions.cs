using Newtonsoft.Json;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.AzureCosmosDb
{
    public class AzureCosmosDbOptions : EntityStoreOptionsBase
    {
        public string CollectionId { get; set; }

        public string AuthKey { get; set; } = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public string DatabaseId { get; set; } = "sopidev";

        public string EndpointUrl { get; set; } = "https://localhost:8081";

        public bool ScaleOfferThroughput { get; set; } = false;

        public int OfferThroughput { get; set; } = 400;

        public int ConcurrentDocuments { get; set; } = 1000;

        public int ConcurrentWorkers { get; set; } = 6;


        public AzureCosmosDbOptions CreateSecured()
        {
            var json = JsonConvert.SerializeObject(this);
            var opt = JsonConvert.DeserializeObject<AzureCosmosDbOptions>(json);
            opt.AuthKey = "XXX";
            return opt;

        }

    }
}
