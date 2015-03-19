using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pixelbyte;

namespace JSONerTests
{
    //I stole most of these tests from:
    //https://github.com/facebook-csharp-sdk/simple-json
    [TestClass]
    public class DesrializationTests
    {
        [TestMethod]
        public void ReadIndented()
        {
            string input = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}";
            var p = PixelJSON.LoadString(input);

            //The array object should be null
            Assert.IsNull(p.array);

            Assert.AreEqual("Intel", p["CPU"]);
            Assert.IsInstanceOfType(p.table["Drives"], typeof(object[]));

            //Check count
            Assert.AreEqual(2, ((object[])p.table["Drives"]).Length);

            //Check data
            Assert.AreEqual("DVD read/writer", p["Drives",0]);
            Assert.AreEqual("500 gigabyte hard drive", p["Drives", 1]);
        }

        [TestMethod]
        public void NestedObjects()
        {
string input = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ],
   ""Old"" : {""Bat"" : ""Masterson""}
}";
            var p = PixelJSON.LoadString(input);
            Assert.AreEqual("Masterson", p["Old","Bat"]);
        }

        [TestMethod]
        public void InvalidJSON()
        {
            var p = PixelJSON.LoadString("hi");

            Assert.IsNull(p.table);
            Assert.IsNull(p.array);
            Assert.IsNull(p["stuff"]);
        }

        [TestMethod]
        public void ReadNullTerminatedString()
        {
            var p = PixelJSON.LoadString("{\"Nuller\":\"h\0i\"}");

            Assert.AreEqual("h\0i", p["Nuller"]);
        }

        [TestMethod]
        public void ReadEmptyObject()
        {
            var p = PixelJSON.LoadString("{}");

            //The array object should be null
            Assert.IsNull(p.array);

            Assert.IsNotNull(p.table);

            Assert.AreEqual(0, p.table.Count);
        }

        [TestMethod]
        public void UnexpectedEndOfHex()
        {
            //For a short hex code, we dont throw an exception
            //we just skip it. TODO: Might change behavior if I use this for anything other than Unity3d stuff.
            var p = PixelJSON.LoadString(@"{ ""t"" : ""'h\u006""}");

            //The array object should be null
            Assert.IsNull(p.array);

            Assert.IsNotNull(p.table);

            Assert.AreEqual("'h", p["t"]);

            Assert.AreEqual(1, p.table.Count);
        }

        [TestMethod]
        public void UnexpectedEndOfControlCharacter()
        {
            var p = PixelJSON.LoadString(@"{""t"" : ""'h\""}");

            //The array object should be null
            Assert.IsNull(p.array);

            Assert.IsNotNull(p.table);

            Assert.AreEqual("'h\"}", p["t"]);

            Assert.AreEqual(1, p.table.Count);
        }

        [TestMethod]
        public void UnexpectedEndWhenParsingUnquotedProperty()
        {
            var p = PixelJSON.LoadString(@"{aww");

            //The array object should be null
            Assert.IsNull(p.array);

            Assert.IsNotNull(p.table);
            Assert.AreEqual(0, p.table.Count);
        }

        [TestMethod]
        public void ParsingQuotedPropertyWithControlCharacters()
        {
            var p = PixelJSON.LoadString("{\"hi\r\nbye\":1}");

            //The array object should be null
            Assert.IsNull(p.array);

            Assert.IsNotNull(p.table);
            Assert.AreEqual(1, p.table.Count);

            foreach (KeyValuePair<string, object> pair in p.table)
            {
                Assert.AreEqual(@"hi
bye", pair.Key);
            }
        }

        [TestMethod]
        public void ReadNewLineLastCharacter()
        {
            string input = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [ 
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}" + '\n';

            var p = PixelJSON.LoadString(input);

            //The array object should be null
            Assert.IsNull(p.array);

            Assert.IsNotNull(p.table);
            Assert.AreEqual(2, p.table.Count);

            Assert.AreEqual("Intel", p["CPU"]);
        }

        [TestMethod]
        public void LongStringTests()
        {
            int length = 20000;
            string json = @"[""" + new string(' ', length) + @"""]";

            var p = PixelJSON.LoadString(json);

            Assert.IsNotNull(p.array);

            Assert.IsNull(p.table);

            Assert.IsInstanceOfType(p.StringFromArray(0), typeof(string));

            Assert.AreEqual(20000, (p.StringFromArray(0)).Length);
        }

        [TestMethod]
        public void EscapedUnicodeTests()
        {
            string json = @"[""\u003c"",""\u5f20""]";

            var p = PixelJSON.LoadString(json);

            Assert.IsNotNull(p.array);

            Assert.IsNull(p.table);

            Assert.AreEqual(2, p.array.Length);

            Assert.AreEqual("<", p.StringFromArray(0));
            //Assert.AreEqual("24352", Convert.ToInt32(Convert.ToChar(l[0])));
        }

        [TestMethod]
        public void MissingColon()
        {
            string json = @"{
    ""A"" : true,
    ""B"" ""hello"", // Notice the colon is missing
    ""C"" : ""bye""
}";

            var p = PixelJSON.LoadString(json);

            Assert.IsNull(p.array);
            Assert.IsNull(p.table);

        }

        [TestMethod]
        public void ReadUnicode()
        {
            string json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

            var p = PixelJSON.LoadString(json);

            Assert.AreEqual(@"Hi,I" + '\u0092' + "ve send you smth", p["Message"]);
        }

        [TestMethod]
        public void ParseIncompleteArray()
        {
            var p = PixelJSON.LoadString("[1");
            //Assert.IsNotNull(p.array);
            //Assert.AreEqual(1, p.array.Length);
            Assert.IsNull(p.array);
            Assert.IsNull(p.table);
        }

        [TestMethod]
        public void DeserializeUnicodeChar()
        {
            string json = "{\"t\" : \"न\"}";

            var p = PixelJSON.LoadString(json);

            Assert.AreEqual("न", p["t"]);
        }

        [TestMethod]
        public void DeserializeSurrogatePair()
        {
            string json = "{\"t\":\"𩸽 is Arabesque greenling(fish) in japanese\"}";
            var p = PixelJSON.LoadString(json);

            Assert.AreEqual("𩸽 is Arabesque greenling(fish) in japanese", p["t"]);
        }

        [TestMethod]
        public void DeserializeEscapedSurrogatePair()
        {
            string json = "{\"t\":\"\\uD867\\uDE3D is Arabesque greenling(fish)\"}";  // 𩸽
            var p = PixelJSON.LoadString(json);

            Assert.AreEqual("\uD867\uDE3D is Arabesque greenling(fish)", p["t"]);
            Assert.AreEqual("𩸽 is Arabesque greenling(fish)", p["t"]);
        }

        [TestMethod]
        public void DeserializeInvaildEscapedSurrogatePair()
        {
            string json = "\"\\uD867\\u0000 is Arabesque greenling(fish)\"";
            var p = PixelJSON.LoadString(json);
            Assert.IsNull(p.array);
            Assert.IsNull(p.table);
        }

        [TestMethod]
        public void DeserializeDoubleQuotesCorrectly()
        {
            var json = "{\"message\":\"Hi \\\"Prabir\\\"\"}";
            var p = PixelJSON.LoadString(json);

            Assert.AreEqual("Hi \"Prabir\"", p["message"]);
        }

        [TestMethod]
        public void DeserializeUriCorrectly()
        {
            var json = "{\"url\":\"https://github.com/shiftkey/simple-json/issues/1\"}";
            var p = PixelJSON.LoadString(json);

            Assert.AreEqual(new Uri("https://github.com/shiftkey/simple-json/issues/1"), p["url"]);
        }

        [TestMethod]
        public void RootWithArray()
        {
            var json = "[45,56,787,3]";
            var p = PixelJSON.LoadString(json);

            Assert.AreEqual("56", p.array[1]);
            Assert.AreEqual("787", p.array[2]);
        }

        [TestMethod]
        public void RootWithArrayofObjects()
        {
            var json = "[{\"cans\":10},{\"gum\":bubble}]";
            var p = PixelJSON.LoadString(json);

            Assert.AreEqual("10", p[0, "cans"]);
            Assert.AreEqual("bubble", p[1, "gum"]);
        }
    }

    [TestClass]
    public class ObjectExtensionsTests
    {
        [TestMethod]
        public void ToTypedArrayExtensions()
        {
            var p = new object[] { 90, 28 };

            var s = p.ToArray<string>();
            var i = p.ToArray<int>();
            var f = p.ToArray<float>();
            var b = p.ToArray<bool>();

            CollectionAssert.AreEqual(new string[] { "90", "28" }, s);
            CollectionAssert.AreEqual(new int[] { 90, 28 }, i);
            CollectionAssert.AreEqual(new float[] { 90, 28 }, f);
            Assert.IsNull(b);

            p = new object[] { "straight", "royal" };
            s = p.ToArray<string>();
            i = p.ToArray<int>();
            f = p.ToArray<float>();
            b = p.ToArray<bool>();

            CollectionAssert.AreEqual(new string[] { "straight", "royal" }, s);
            Assert.IsNull(i);
            Assert.IsNull(f);
            Assert.IsNull(b);
        }
    }
}