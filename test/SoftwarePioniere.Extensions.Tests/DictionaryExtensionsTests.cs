using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Extensions.Tests
{
    public class DictionaryExtensionsTests
    {
        [Fact]
        public void EnsureDictArrayContainsValueTest()
        {
            var dict = new Dictionary<string, string[]>
            {
                {"a", new []{ "aa", "aaa"}},
                {"b", new []{ "bb", "bbb"}}
            };

            dict.EnsureDictArrayContainsValue("a", "aaaa");

            dict.Should().ContainKey("a");
            dict["a"].Should().Contain("aa");
            dict["a"].Should().Contain("aaa");
            dict["a"].Should().Contain("aaaa");

        }

        [Fact]
        public void EnsureDictArrayNotContainsValueTest()
        {
            var dict = new Dictionary<string, string[]>
            {
                {"a", new []{ "aa", "aaa","aaaa"}},
                {"b", new []{ "bb", "bbb"}}
            };

            dict.EnsureDictArrayNotContainsValue("a", "aaa");

            dict.Should().ContainKey("a");
            dict["a"].Should().Contain("aa");
            dict["a"].Should().NotContain("aaa");

        }

        [Fact]
        public void EnsureDictNotContainsKeyTest()
        {
            var dict = new Dictionary<string, string[]>
            {
                {"a", new []{ "aa", "aaa","aaaa"}},
                {"b", new []{ "bb", "bbb"}}
            };
            dict.EnsureDictNotContainsKey("a");
            dict.Should().NotContainKey("a");

            var dict1 = new Dictionary<string, string>
            {
                {"a",  "aa"},
                {"b", "bb"}
            };
            dict1.EnsureDictNotContainsKey("a");
            dict1.Should().NotContainKey("a");
        }

        [Fact]
        public void EnsureDictContainsValueTest()
        {
            var dict = new Dictionary<string, string[]>
            {
                {"a", new []{ "aa", "aaa"}},
                {"b", new []{ "bb", "bbb"}}
            };
            dict.Should().ContainKey("a");
            dict["a"].Should().Contain("aa");
            dict["a"].Should().Contain("aaa");
            dict["a"].Should().NotContain("aaaa");

            dict = dict.EnsureDictContainsValue("a", new[] { "aa", "aaa", "aaaa" });
            dict.Should().ContainKey("a");

            dict["a"].Should().Contain("aa");
            dict["a"].Should().Contain("aaa");
            dict["a"].Should().Contain("aaaa");

        }

        [Fact]
        public void EnsureDictContainsValueWithActionTest()
        {
            var dict = new Dictionary<string, Class1>
            {
                {"a", new Class1("aa")},
                {"b", new Class1("bb")}
            };
            dict.Should().ContainKey("a");
            dict["a"].Text.Should().Be("aa");

            dict = dict.EnsureDictContainsValue("a",
                class1 =>
                    class1.Text = "aaa");

            dict.Should().ContainKey("a");
            dict["a"].Text.Should().Be("aaa");

        }

        private class Class1
        {
            public Class1()
            {

            }

            public string Text { get; set; }

            public Class1(string text)
            {
                Text = text;
            }

        }
    }
}