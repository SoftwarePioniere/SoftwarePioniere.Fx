using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SoftwarePioniere.ReadModel
{
    public class MyEntity1 : Entity
    {
        public const string TypeKey = "My:Entity1";

        public MyEntity1() : base(TypeKey)
        {
        }

        [JsonProperty("string_value")]
        public string StringValue { get; set; }
    }

    public class FakeEntity : Entity
    {
        public const string TypeKey = "My:FakeEntity";

        public FakeEntity() : base(TypeKey)
        {
            Children = new List<FakeEntity>();
        }

        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public Guid GuidValue { get; set; }
        public DateTime DateTimeValueUtc { get; set; }
        public double DoubleValue { get; set; }

        public IList<FakeEntity> Children { get; set; }

        public string ChunkId { get; set; }

        public static FakeEntity Empty()
        {
            return new FakeEntity();
        }

        public override string ToString()
        {
            return $"{IntValue} {StringValue}";
        }

        public static FakeEntity Create(string id)
        {
            var ent = new FakeEntity
            {
                DateTimeValueUtc = DateTime.UtcNow,
                DoubleValue = 122.33,
                IntValue = 112,
                GuidValue = Guid.NewGuid(),
                StringValue = "string 1"
            };

            ent.SetEntityId(id);

            return ent;
        }

        public static IEnumerable<FakeEntity> CreateList(int count)
        {
            var chunkid = Guid.NewGuid().ToString();
            for (var i = 0; i < count; i++)
            {
                var ent = Create(Guid.NewGuid().ToString());
                ent.ChunkId = chunkid;
                ent.IntValue += i + 1;
                ent.StringValue = $"string {i + 1}";
                yield return ent;
            }
        }

        public static IEnumerable<FakeEntity> CreateList2(int count)
        {
            var chunkid = Guid.NewGuid().ToString();
            for (var i = 0; i < count; i++)
            {
                var ent = Create(Guid.NewGuid().ToString());
                ent.ChunkId = chunkid;
                ent.IntValue = 0;
                ent.StringValue = $"string { 100 * count - i}";
                yield return ent;
            }
        }
    }
}
