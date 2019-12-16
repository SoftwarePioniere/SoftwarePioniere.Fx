using Newtonsoft.Json;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.MongoDb
{
    public class MongoDbOptions : EntityStoreOptionsBase
    {

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DatabaseId { get; set; } 

        public string Server { get; set; } = "localhost";

        public int Port { get; set; } = 27017;

        public override string ToString()
        {
            return $"Server: {Server} // Port: {Port} // DatabaseId: {DatabaseId} // UserName: {UserName} ";
        }

        public MongoDbOptions CreateSecured()
        {
            var json = JsonConvert.SerializeObject(this);
            var opt = JsonConvert.DeserializeObject<MongoDbOptions>(json);
            opt.Password = "XXX";
            return opt;
        }
    }
}
