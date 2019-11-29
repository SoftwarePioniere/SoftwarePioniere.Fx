using System;
using System.Collections.Generic;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.FakeDomain
{
    public class FakeEntity : Entity
    {
        public const string TypeKey = "My:FakeEntity";

        public FakeEntity() : base(TypeKey)
        {
          
        }

        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public Guid GuidValue { get; set; }
        public DateTime DateTimeValueUtc { get; set; }
        public double DoubleValue { get; set; }

        public List<FakeEntity> Children { get; set; } = new List<FakeEntity>();

        public string ChunkId { get; set; }
        public Dictionary<string, string> Dict1 { get; set; } = new Dictionary<string, string>();

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
