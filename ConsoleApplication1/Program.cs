using System;
using System.Collections.Generic;
using System.Text;
using Pixelbyte;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var z = new object[] { "straight", "royal" };
            var s = z.ToArray<string>();
            //var p = PJSON.LoadString("{\"Tst\": 1.03e4 \"night\":9,\"Lost Weight\": [true,false,true,true] , \"Tap\":{\"Filer\":90} } ");
            //var p = PJSON.LoadString("[{\"firstName\":\"John\", \"lastName\":\"Doe\"},{\"firstName\":\"Anna\", \"lastName\":\"Smith\"},{\"firstName\":\"Peter\",\"lastName\":\"Jones\"}]");
            //var p = PJSON.LoadString("[45,67,34,21,32,1]");

            //var p = PixelJSON.Load(@"F:\untitled.json");
            //string json = @"{ ""t"" : ""'h\u006""}";
            //string json = @"{ ""A"" : true,""B"" ""hello"", // Notice the colon is missing""C"" : ""bye""}";
            //string json = "[1";
            string json = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ],
   ""Old"" : {""Bat"" : ""Masterson""}
}";
            var p = PixelJSON.LoadString(json);

            var str = PixelJSONSerializer.Serialize(p, true, false);
            Console.WriteLine(str);
            //var p = JSONer.LoadString(@"{""t"" : ""'h\""}");
            //var p = JSON.LoadString(@"{aww");
            //string json = @"[""\u003c"",""\u5f20""]";
            //var p = PJSON.LoadString(json);
            //Is our root item an object?
            //if (p.table != null)
            //{
            //    Console.WriteLine(p["height"]);
            //}
            //else //Or is it an array?
            //{
            //    Console.WriteLine(p[0, "firstName"]);
            //    Console.WriteLine(p[0, "lastName"]);
            //}

            //Test serializer
            //using (var sw = new StreamWriter(@"F:\out.txt"))
            //{
            //    sw.WriteLine(PixelJSONSerializer.Serialize(p,true));
            //}

            //Console.WriteLine(p.root.ToArray<int>()[0]);
            //Console.WriteLine(p.root[0]["firstName"].value);
            //	Console.WriteLine (p.main["Tst"].value);
            //	Console.WriteLine (p.main["night"].value);
            //	Console.WriteLine (p.main["Lost Weight"].ArrB());
            //	Console.WriteLine (p.main["Tap"].obj["Filer"].value);
        }
    }
}
