using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

// My JSON Parser
// 
// 2015 Pixelbyte Studios
/*
 * Based on MiniJSON.cs by Calvin Rien | https://gist.github.com/darktable/1411710
 * which was based off of the JSON parser by Patrick van Bergen | http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 *
 * There might also be some snippets from WyrmTale Games | https://www.wyrmtale.com/blog/2013/98/json-formatting-and-parsing-in-unity3d
 * in here too.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
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
namespace Pixelbyte.IO
{
    enum Token
    {
        //6 Structural Tokens
        SquareOpen, SquareClose, CurlyOpen, CurlyClose, Colon, Comma,
        //Plus the 3 literal name tokens
        True, False, Null,
        //And a few others
        DoubleQuote, Digit, None,
    }

    public static class JSONExtensions
    {
        /// <summary>
        /// Converts the array to a typed array
        /// Wont do anything if the array field is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ToArray<T>(this object obj)
        {
            object[] array = obj as object[];
            if (array == null) return null;

            try
            {
                List<T> l = new List<T>(array.Length);

                if (l is List<string>)
                {
                    List<string> ls = l as List<string>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(array[i].ToString());
                }
                else if (l is List<int>)
                {
                    List<int> ls = l as List<int>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(System.Convert.ToInt32(array[i]));
                }
                else if (l is List<float>)
                {
                    List<float> ls = l as List<float>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(System.Convert.ToSingle(array[i]));
                }
                else if (l is List<bool>)
                {
                    List<bool> ls = l as List<bool>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(bool.Parse(array[i].ToString()));
                }
                return l.ToArray();
            }
            catch (Exception) { return null; } //for now we just eat the exception
        }

        public static object GetO(this Dictionary<string, object> dict, string key)
        {
            object obj = null;
            dict.TryGetValue(key, out obj);
            return obj;
        }

        public static Dictionary<string, object>[] GetTables(this Dictionary<string, object> dict, string key)
        {
            object obj = null;
            if (dict.TryGetValue(key, out obj))
            {
                List<Dictionary<string, object>> tables = new List<Dictionary<string, object>>();

                var objs = obj as object[];
                if (objs != null)
                {
                    for (int i = 0; i < objs.Length; i++)
                    {
                        var table = objs[i] as Dictionary<string, object>;
                        if (table != null)
                            tables.Add(table);
                    }
                    if (tables.Count == 0) return null;
                    return tables.ToArray();
                }
            }
            return null;
        }

        public static string GetS(this Dictionary<string, object> dict, string key, string defVal = null)
        {
            if (!dict.TryGetValue(key, out object obj)) return defVal;
            else return obj.ToString();
        }

        public static float GetF(this Dictionary<string, object> dict, string key, float defVal = float.NaN)
        {
            if (!dict.TryGetValue(key, out object obj)) return defVal;
            else
            {
                float val;
                if (!float.TryParse(obj.ToString(), out val)) return defVal;
                else return val;
            }
        }

        public static int GetI(this Dictionary<string, object> dict, string key, int defVal = int.MinValue)
        {
            if (!dict.TryGetValue(key, out object obj)) return defVal;
            else
            {
                int val;
                if (!int.TryParse(obj.ToString(), out val)) return defVal;
                else return val;
            }
        }

        public static bool GetB(this Dictionary<string, object> dict, string key, bool defVal = false)
        {
            if (!dict.TryGetValue(key, out object obj)) return defVal;
            else
            {
                bool val;
                if (!bool.TryParse(obj.ToString(), out val)) return defVal;
                else return val;
            }
        }
    }

    public class PixelJSON
    {
        //JSON spec defines whitespace as the following:
        const string WHITESPACE = " \t\n\r";
        const string WORDBREAKS = WHITESPACE + "[]{}:,\"";

        TextReader _reader;

        //The root JsonValue 
        public Dictionary<string, object> table;
        public object[] array;

        /// <summary>
        /// Gets a string result from a nested object in an object
        /// This assumes the table is valid
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] => GetValue(key); 
        public string this[string key, string key2] => GetValue(key, key2);
        public string this[string key, string key2, string key3] => GetValue(key, key2, key3);
        public string this[string key, string key2, string key3, string key4] => GetValue(key, key2, key3, key4);

        /// <summary>
        /// Given any number of keys, this gets the corresponding value as a
        /// string. If any of the keys dont exist, or all but the last object
        /// is not a Dictionary this return null
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>string representation of the value if the keys are valid, null otherwise</returns>
        string GetValue(params string[] keys)
        {
            Dictionary<string, object> tble = table;
            for (int i = 0; i < keys.Length; i++)
            {
                if (tble == null || !tble.ContainsKey(keys[i])) return null;

                if (i < keys.Length - 1)
                    tble = tble[keys[i]] as Dictionary<string, object>;
                else
                    return tble[keys[i]].ToString();
            }
            return null;
        }

        public Dictionary<string, object> GetTable(string key)
        {
            if (table.TryGetValue(key, out object obj))
            {
                var d = obj as Dictionary<string, object>;
                return d;
            }
            else
                return null;
        }

        public Dictionary<string, object>[] GetTables(string key)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            if (table.TryGetValue(key, out object obj))
            {
                var d = obj as Dictionary<string, object>;
                if (d == null)
                {
                    var arr = obj as object[];
                    if (arr == null) return null;

                    for (int i = 0; i < arr.Length; i++)
                    {
                        d = arr[i] as Dictionary<string, object>;
                        if (d != null)
                            list.Add(d);
                    }
                }
                else
                    list.Add(d);

                if (list.Count > 0) return list.ToArray();
                else return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Gets a string from the named JSON object that is an array
        /// </summary>
        /// <param name="?"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[string key, int index]
        {
            get
            {
                if (table == null || !table.ContainsKey(key)) return null;
                object[] anArray = table[key] as object[];
                if (anArray == null || anArray.Length <= index) return null;
                else return anArray[index].ToString();
            }
            set
            {
                if (table == null || !table.ContainsKey(key)) return;
                object[] anArray = table[key] as object[];
                if (anArray == null || anArray.Length <= index) return;
                else anArray[index] = value;
            }
        }

        /// <summary>
        /// Gets the specified indexed item in the array and assumes it is
        /// itself a json object of the form Dictionary<string, object>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Dictionary<string, object> this[int index]
        {
            get
            {
                if (array == null || index >= array.Length) return null;
                else return array[index] as Dictionary<string, object>;
            }
        }

        /// <summary>
        /// Gets a specific indexed object and gets the value for the given key
        /// NOTE: assumes that the array field is full of Objects (Dictionary<string, object>)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[int index, string key]
        {
            get
            {
                if (array == null || index >= array.Length) return null;
                else
                {
                    var dict = array[index] as Dictionary<string, object>;
                    if (dict == null || !dict.ContainsKey(key)) return null;
                    return dict[key].ToString();
                }
            }
        }

        /// <summary>
        /// Converts the array to a typed array
        /// Wont do anything if the array field is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] ToArray<T>()
        {
            if (array == null || array.Length == 0) return null;
            try
            {
                List<T> l = new List<T>(array.Length);

                if (l is List<string>)
                {
                    List<string> ls = l as List<string>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(array[i].ToString());
                }
                else if (l is List<int>)
                {
                    List<int> ls = l as List<int>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(System.Convert.ToInt32(array[i]));
                }
                else if (l is List<float>)
                {
                    List<float> ls = l as List<float>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(System.Convert.ToSingle(array[i]));
                }
                else if (l is List<bool>)
                {
                    List<bool> ls = l as List<bool>;
                    for (int i = 0; i < array.Length; i++)
                        ls.Add(System.Convert.ToBoolean(array[i]));
                }
                return l.ToArray();
            }
            catch (Exception) { return null; } //for now we just eat the exception
        }

        /// <summary>
        /// Gets the specific indexed item in the array as a string
        /// Note: 
        /// 1) Assumes the deserialized data is stored in the array field
        /// 2)  there are no Objects in the array if there are, well this wont work
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string StringFromArray(int index)
        {
            if (array == null || index >= array.Length) return null;
            else return array[index].ToString();
        }

        Token NextToken
        {
            get
            {
                if (_reader == null) return Token.None;
                EatWhitespace();
                if (EOF()) return Token.None;
                else return Tokenize(PeekChar());
            }
        }

        private PixelJSON() { }

        public static PixelJSON LoadString(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            var jsoner = new PixelJSON();
            jsoner.ParseString(data);
            return jsoner;
        }

        public static PixelJSON Load(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            var jsoner = new PixelJSON();
            jsoner.ParseFromFile(filename);
            return jsoner;
        }

        private void ParseFromFile(string filename)
        {
            using (_reader = new StreamReader(filename))
            {
                ParseLoop();
            }
            _reader = null;
        }

        private void ParseString(string data)
        {
            using (_reader = new StringReader(data))
            {
                ParseLoop();
            }
            _reader = null;
        }

        private void ParseLoop()
        {
            Token t;
            while ((t = NextToken) != Token.None)
            {
                if (t == Token.CurlyOpen)
                {
                    table = ParseObject();
                }
                else if (t == Token.SquareOpen)
                {
                    array = ParseArray();
                }
                else
                    Console.Write(NextChar());
            }
        }

        char NextChar() { return Convert.ToChar(_reader.Read()); }
        char PeekChar() { return Convert.ToChar(_reader.Peek()); }
        bool EOF() { return _reader.Peek() == -1; }

        void EatWhitespace()
        {
            while (_reader.Peek() > -1 && WHITESPACE.IndexOf(PeekChar()) > -1) _reader.Read();
        }

        string ParseWord()
        {
            StringBuilder sb = new StringBuilder();
            while (WORDBREAKS.IndexOf(PeekChar()) == -1)
            {
                sb.Append(NextChar());
                if (EOF()) break;
            }
            return sb.ToString();
        }

        string ParseString()
        {
            char c;

            //Eat the opening quote
            NextChar();

            StringBuilder sb = new StringBuilder();
            while (_reader.Peek() != -1 && PeekChar() != '"')
            {
                c = NextChar();

                //Check for specials
                switch (c)
                {
                    case '\\': //Escape sequence according to JSON spec
                        if (EOF()) break;
                        c = NextChar();
                        switch (c) //Do valid JSON string escape chars
                        {
                            case '"':
                            case '/':  //solidus
                            case '\\': //reverse solidus
                                sb.Append(c);
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'u': //4 hexadecimal digits
                                var hx = new StringBuilder();
                                //Next 4 chars should represent a unicode char
                                for (int i = 0; i < 4; i++)
                                {
                                    if (EOF() || !IsHex(PeekChar())) break;
                                    hx.Append(NextChar());
                                }
                                if (hx.Length == 4)
                                    sb.Append((char)Convert.ToInt32(hx.ToString(), 16));
                                //else
                                //TODO: an exception?
                                break;
                        }
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            //Eat the closing quote (added EOF() test to not crash on bad strings that have no end quote
            if (!EOF())
                NextChar();

            return sb.ToString();
        }

        Token Tokenize(char c)
        {
            switch (c)
            {
                case '[': return Token.SquareOpen;
                case ']': return Token.SquareClose;
                case '{': return Token.CurlyOpen;
                case '}': return Token.CurlyClose;
                case ':': return Token.Colon;
                case ',': return Token.Comma;
                case '"': return Token.DoubleQuote;
                default:
                    if ((c >= '0' && c <= '9') || c == '-') return Token.Digit;
                    break;
            }

            //Not a single char token? try one of the 3 named tokens
            char pk = Char.ToLower(PeekChar());
            if (pk == 'n' || pk == 't' || pk == 'f')
            {
                string w = ParseWord();
                switch (w.ToLower())
                {
                    case "null": return Token.Null;
                    case "true": return Token.True;
                    case "false": return Token.False;
                }
            }

            //Didn't find a token...
            return Token.None;
        }

        Dictionary<string, object> ParseObject()
        {
            //Eat the '{'
            NextChar();

            var job = new Dictionary<string, object>();

            while (true)
            {
                //JSON spec says to expect a string : value with optional ,
                switch (NextToken)
                {
                    case Token.None:
                        return job; //TODO: Return the object
                    case Token.CurlyClose:
                        //eat the closing curly
                        NextChar();
                        return job;
                    case Token.Comma: //A comma here means another object
                        NextChar();
                        continue;
                    default:
                        //Expect a string : value pair
                        string name = ParseString();

                        //Name must be valid, then we must have a ':'
                        if (name == null || NextToken != Token.Colon)
                        {
                            //TOOD: Log it..
                            //continue;
                            return null;
                        }

                        //Eat the colon
                        NextChar();

                        //Ok then this must be a string/value pair
                        object val = ParseValue();

                        job[name] = val;
                        break;
                }
            }
        }

        private object ParseValue()
        {
            object val = null;

            //Normally the tokens are not eaten, but
            //if it is one of the 3 literals, it is
            Token t = NextToken;
            switch (t)
            {
                case Token.DoubleQuote:
                    val = ParseString();
                    break;
                case Token.True:
                    val = "true";
                    break;
                case Token.False:
                    val = "false";
                    break;
                case Token.Null:
                    val = "null";
                    break;
                case Token.SquareOpen:
                    return ParseArray();
                case Token.CurlyOpen:
                    return ParseObject();
                case Token.Digit:
                    //Mmm a number
                    val = ParseWord();
                    break;
                default:
                    val = ParseWord();
                    break;
            }
            return val;
        }

        object[] ParseArray()
        {
            //This is used when we are parsing an array
            List<object> temp = new List<object>(40);

            //Eat the opening '['
            NextChar();

            Token t;
            object val = null;
            while ((t = NextToken) != Token.SquareClose)
            {
                switch (t)
                {
                    case Token.CurlyOpen:
                        val = ParseObject();
                        break;
                    case Token.Comma:
                        NextChar();
                        continue;
                    case Token.DoubleQuote:
                        val = ParseString();
                        break;
                    case Token.True:
                        val = "true";
                        break;
                    case Token.False:
                        val = "false";
                        break;
                    case Token.Null:
                        val = "null";
                        break;
                    case Token.Digit:
                        //Mmm a number
                        val = ParseWord();
                        break;
                    case Token.None:
                        return null;
                    //return temp.ToArray(); If you want fault tolerance, uncomment this instead
                    default:
                        val = ParseWord();
                        break;
                }
                temp.Add(val);
            }

            //eat the closing ']'
            NextChar();

            return temp.ToArray();
        }

        bool IsHex(char c)
        {
            return ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }
    }
}