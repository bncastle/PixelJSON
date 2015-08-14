using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte
{
    //
    // PJSON Serializer
    // 
    // 2015 Pixelbyte Studios
    //
    // Started out as the JSON parser serializer by Patrick van Bergen | http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
    // and some of JSON.cs by WyrmTale Games | https://www.wyrmtale.com/blog/2013/98/json-formatting-and-parsing-in-unity3d
    //
    /* Permission is hereby granted, free of charge, to any person obtaining
    * a copy of this software and associated documentation files (the
    * "Software"), to deal in the Software without restriction, including
    * without limitation the rights to use, copy, modify, merge, publish,
    * distribute, sublicense, and/or sell copies of the Software, and to
    * permit persons to whom the Software is furnished to do so, subject to
    * the following conditions:
    *
    * The above copyright notice and this permission notice shall be
    * included in all copies or substantial portions of the Software.
    *
    * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */
    public class PixelJSONSerializer
    {
        StringBuilder builder;
        int level = 0;
        bool multiline = true;
        bool multilineArrays = false;

        public PixelJSONSerializer(bool multiline = true, bool multilineArrays = false)
        {
            builder = new StringBuilder();
            level = 0;
            this.multiline = multiline;
            this.multilineArrays = multilineArrays;
        }

        public static string Serialize(PixelJSON json, bool multiline = true, bool multilineArrays = false)
        {
            var instance = new PixelJSONSerializer(multiline, multilineArrays);

            if (json.table != null)
                instance.SerializeObject(json.table);
            else if (json.array != null)
                instance.SerializeArray(json.array);

            return instance.builder.ToString();
        }

        static bool IsNumber(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsDigit(text[i]) && text[i] != '-' && text[i] != '.') { return false; }
            }
            return true;
        }

        void SerializeValue(object value)
        {
            if (value == null) builder.Append("null");
            else if (value is string)
            {
                string st = (value as string).ToLower();

                //Don't quote numbers or primitive value
                if (st == "null" || st == "true" || st == "false" || IsNumber(st))
                    builder.Append(st);
                else
                    SerializeString(value as string);
            }
            else if (value is bool)
            {
                builder.Append(value.ToString().ToLower());
            }
            else if (value as IDictionary != null)
            {
                SerializeObject(value as IDictionary);
            }
            else if (value as IList != null)
            {
                SerializeArray(value as IList);
            }
            else if (value is char)
            {
                SerializeString(value.ToString());
            }
            else
            {
                SerializeOther(value);
            }
        }

        void SerializeObject(IDictionary obj)
        {
            bool first = true;

            level++;

            builder.Append('{');

            if (obj.Keys.Count > 1)
            {
                builder.AppendLine();
                builder.Append('\t', level);
            }


            foreach (object e in obj.Keys)
            {
                if (!first)
                {
                    builder.Append(',');
                    builder.Append('\n');
                    builder.Append('\t', level);
                }

                SerializeString(e.ToString());
                builder.Append(" : ");

                SerializeValue(obj[e]);

                first = false;
            }

            level--;

            if (!first && multiline && obj.Keys.Count > 1)
            {
                builder.Append('\n');
                builder.Append('\t', level);
            }
            builder.Append('}');
        }

        void SerializeArray(IList anArray)
        {
            level++;

            if (anArray.Count > 1)
            {
                builder.AppendLine();

                if (level - 1 > 0)
                    builder.Append('\t', level - 1);
            }
            builder.Append('[');

            bool first = true;

            foreach (object obj in anArray)
            {
                if (!first)
                {
                    //builder.Append(',');
                    builder.Append(", ");
                }
                if (multilineArrays)
                {
                    builder.Append('\n');
                    builder.Append('\t', level);
                }

                SerializeValue(obj);

                first = false;
            }

            level--;

            if (!first && multilineArrays && anArray.Count > 1)
            {
                builder.Append('\n');
                builder.Append('\t', level);
            }
            builder.Append(']');
        }

        void SerializeString(string str)
        {
            builder.Append('\"');

            char[] charArray = str.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"') builder.Append("\\\"");
                else if (c == '\\') builder.Append("\\\\");
                else if (c == '\b') builder.Append("\\b");
                else if (c == '\f') builder.Append("\\f");
                else if (c == '\n') builder.Append("\\n");
                else if (c == '\r') builder.Append("\\r");
                else if (c == '\t') builder.Append("\\t");
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126)) builder.Append(c);
                    else builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                }
            }
            builder.Append('\"');
        }

        void SerializeOther(object value)
        {
            if (value is float || value is int || value is uint || value is long
                || value is double || value is sbyte || value is byte || value is short
                || value is ushort || value is ulong || value is decimal)
            {
                builder.Append(value.ToString());
            }
            else
            {
                SerializeString(value.ToString());
            }
        }
    }
}
