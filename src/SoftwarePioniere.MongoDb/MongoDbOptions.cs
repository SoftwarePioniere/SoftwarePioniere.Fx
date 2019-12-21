using System;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json;
using SoftwarePioniere.ReadModel;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SoftwarePioniere.MongoDb
{
    public class MongoDbOptions : EntityStoreOptionsBase
    {

        public bool UseTelemetry { get; set; }

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
            return new MongoDbOptions
            {
                DatabaseId = DatabaseId,
                Port = Port,
                Server = Server,
                UserName = UserName
            };

        }

        [JsonIgnore]
        public Action<ClusterBuilder> ClusterConfigurator { get; set; }
    }
}
