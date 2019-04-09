using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace SoftwarePioniere.Extensions.Tests
{
    public class HashKeyTests
    {
        public class HashKey
        {

            public IDictionary<string, string[]> Items { get; private set; }
            public HashKey()
            {
                Items = new Dictionary<string, string[]>();
            }

            public void Add(string key, object value)
            {
                if (Items.ContainsKey(key))
                {
                    Items.Remove(key);
                }

                if (value != null)
                {
                    Items.Add(key, new[] { value.ToString() });
                }
                else
                { 
                    Items.Add(key, new[] { "NULL" });
                }

            }

            public void AddItems(string key, object[] values)
            {
                if (Items.ContainsKey(key))
                {
                    Items.Remove(key);
                }

                Items.Add(key, values.Where(x => x != null).Select(x => x.ToString()).ToArray());
            }

            private static JsonSerializer _ser;
            public int CreateHash()
            {
                if (_ser == null)
                {
                    _ser = JsonSerializer.Create(new JsonSerializerSettings()
                    {
                        Formatting = Formatting.None
                    });
                }
                var jt = JToken.FromObject(Items, _ser);
                var code = jt.GetHashCode();
                return code;
            }

            public string CreateHash2()
            {
               
                string input = JsonConvert.SerializeObject(Items, Formatting.None);

                var hash = new StringBuilder();
                var md5Provider = new MD5CryptoServiceProvider();
                byte[] bytes = md5Provider.ComputeHash(new UTF8Encoding().GetBytes(input));

                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.Append(bytes[i].ToString("x2"));
                }
                return hash.ToString();
            }
        }

        private HashKey CreateHk()
        {
            var hk = new HashKey();
            hk.Add("K", "entitykey1");
            hk.Add("A", "abc");
            hk.AddItems("ids", new object[] { "id1", "id2", "id3" });


            return hk;
        }

        //[Fact]
        //public void TestHashEquals()
        //{
        //    var hk1 = CreateHk();
        //    var code1 = hk1.CreateHash();

        //    code1.Should().NotBe(0);

        //    var hk2 = CreateHk();
        //    var code2 = hk2.CreateHash();
        //    code2.Should().NotBe(0);

        //    code1.Should().Be(code2);

        //}


        [Fact]
        public void TestHash2Equals()
        {
            var hk1 = CreateHk();
            var code1 = hk1.CreateHash2();

            code1.Should().NotBeNullOrEmpty();

            var hk2 = CreateHk();
            var code2 = hk2.CreateHash2();
            code2.Should().NotBeNullOrEmpty();

            code1.Should().Be(code2);

        }
    }
}
