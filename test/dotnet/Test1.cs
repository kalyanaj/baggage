using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTelemetry;

namespace BaggageTests
{
    [TestClass]
    public class BaggageTests
    {
        [TestMethod]
        public void TestCtorDefault()
        {
            var baggage = Baggage.Create();
            Assert.AreEqual(0, baggage.Count);
        }

        [TestMethod]
        public void TestParseSimple()
        {
            var baggage = Baggage.Create(new Dictionary<string, string> { { "SomeKey", "SomeValue" } });
            Assert.AreEqual(1, baggage.Count);
            Assert.AreEqual("SomeValue", baggage.GetBaggage("SomeKey"));
        }

        [TestMethod]
        public void TestParseMultiple()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue" },
                { "SomeKey2", "SomeValue2" }
            });

            Assert.AreEqual(2, baggage.Count);
            Assert.AreEqual("SomeValue", baggage.GetBaggage("SomeKey"));
            Assert.AreEqual("SomeValue2", baggage.GetBaggage("SomeKey2"));
        }

        [TestMethod]
        public void TestParseWithOWS()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { " SomeKey ", " SomeValue " },
                { " SomeKey2 ", " SomeValue2 " }
            });

            Assert.AreEqual(2, baggage.Count);
            Assert.AreEqual(" SomeValue ", baggage.GetBaggage(" SomeKey "));
            Assert.AreEqual(" SomeValue2 ", baggage.GetBaggage(" SomeKey2 "));
        }

        [TestMethod]
        public void TestParseWithKeyValueProperties()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue;SomeProp=PropVal" },
                { "SomeKey2", "SomeValue2;AnotherProp=AnotherVal" }
            });

            Assert.AreEqual("SomeValue;SomeProp=PropVal", baggage.GetBaggage("SomeKey"));
            Assert.AreEqual("SomeValue2;AnotherProp=AnotherVal", baggage.GetBaggage("SomeKey2"));
        }

        [TestMethod]
        public void TestParseWithKeyValuePropertiesAndOWS()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { " SomeKey ", " SomeValue;SomeProp=PropVal " },
                { " SomeKey2 ", " SomeValue2;AnotherProp=AnotherVal " }
            });

            Assert.AreEqual(" SomeValue;SomeProp=PropVal ", baggage.GetBaggage(" SomeKey "));
            Assert.AreEqual(" SomeValue2;AnotherProp=AnotherVal ", baggage.GetBaggage(" SomeKey2 "));
        }
    }

    [TestClass]
    public class BaggageEntryTests
    {
        [TestMethod]
        public void TestCtorDefault()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>());
            Assert.AreEqual(0, baggage.Count);
        }

        [TestMethod]
        public void TestParseSimpleEntry()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue" }
            });

            Assert.AreEqual("SomeValue", baggage.GetBaggage("SomeKey"));
        }

        [TestMethod]
        public void TestParseMultipleEquals()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue=equals" }
            });

            Assert.AreEqual("SomeValue=equals", baggage.GetBaggage("SomeKey"));
        }

        [TestMethod]
        public void TestParseWithPercentEncoding()
        {
            var encodedValue = Uri.EscapeDataString("\t \"\';=asdf!@#$%^&*()");
            var baggage = Baggage.Create(new Dictionary<string, string> { { "SomeKey", encodedValue } });

            Assert.AreEqual(Uri.UnescapeDataString(encodedValue), baggage.GetBaggage("SomeKey"));
        }

        [TestMethod]
        public void TestParseProperties()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue;SomeProp" }
            });

            Assert.AreEqual("SomeValue;SomeProp", baggage.GetBaggage("SomeKey"));
        }

        [TestMethod]
        public void TestParseMultipleProperties()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue;SomeProp;SecondProp=PropValue" }
            });

            Assert.AreEqual("SomeValue;SomeProp;SecondProp=PropValue", baggage.GetBaggage("SomeKey"));
        }

        [TestMethod]
        public void TestParseKeyValueProperty()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
        {
            { "SomeKey", "SomeValue;SomePropKey=SomePropValue" }
        });

            Assert.AreEqual("SomeValue;SomePropKey=SomePropValue", baggage.GetBaggage("SomeKey"));
        }

        [TestMethod]
        public void TestParseSimpleWithOWS()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
        {
            { " SomeKey ", " SomeValue " }
        });

            Assert.AreEqual(" SomeValue ", baggage.GetBaggage(" SomeKey "));
        }

        [TestMethod]
        public void TestParsePercentEncodedWithOWS()
        {
            var encodedValue = Uri.EscapeDataString("\t \"\';=asdf!@#$%^&*()");
            var baggage = Baggage.Create(new Dictionary<string, string>
        {
            { " SomeKey ", $" {encodedValue} " }
        });

            Assert.AreEqual($" {Uri.UnescapeDataString(encodedValue)} ", baggage.GetBaggage(" SomeKey "));
        }

        [TestMethod]
        public void TestParsePropertyWithOWS()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
        {
            { " SomeKey ", " SomeValue ; SomeProp " }
        });

            Assert.AreEqual(" SomeValue ; SomeProp ", baggage.GetBaggage(" SomeKey "));
        }

        [TestMethod]
        public void TestParseMultiplePropertiesWithOWS()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
        {
            { " SomeKey ", " SomeValue ; SomeProp ; SecondProp = PropValue " }
        });

            Assert.AreEqual(" SomeValue ; SomeProp ; SecondProp = PropValue ", baggage.GetBaggage(" SomeKey "));
        }

        [TestMethod]
        public void TestParseKeyValuePropertyWithOWS()
        {
            var baggage = Baggage.Create(new Dictionary<string, string>
        {
            { " SomeKey ", " SomeValue ; SomePropKey = SomePropValue " }
        });

            Assert.AreEqual(" SomeValue ; SomePropKey = SomePropValue ", baggage.GetBaggage(" SomeKey "));
        }

    }

    [TestClass]
    public class LimitsTests
    {
        [TestMethod]
        public void TestSerializeAtLeast64Entries()
        {
            var entries = new Dictionary<string, string>();
            for (int i = 0; i < 64; i++)
            {
                entries[$"key{i}"] = "value";
            }

            var baggage = Baggage.Create(entries);
            Assert.AreEqual(64, baggage.Count);
        }

        [TestMethod]
        public void TestSerializeLongEntry()
        {
            string longValue = new string('a', 8192 - 5); // Account for "key=a" format
            var baggage = Baggage.Create(new Dictionary<string, string> { { "key", longValue } });

            string serializedBaggage = SerializeBaggage(baggage);
            Assert.AreEqual(8192, serializedBaggage.Length);
        }

        [TestMethod]
        public void TestSerializeManyEntries()
        {
            var entries = new Dictionary<string, string>();
            for (int i = 0; i < 512; i++)
            {
                entries[$"{i:000}"] = "0123456789a";
            }

            var baggage = Baggage.Create(entries);
            var serializedBaggage = SerializeBaggage(baggage) + "b"; // Add trailing byte
            Assert.AreEqual(8192, serializedBaggage.Length);
        }

        private string SerializeBaggage(Baggage baggage)
        {
            return string.Join(",", baggage.GetBaggage().Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
    }
}
