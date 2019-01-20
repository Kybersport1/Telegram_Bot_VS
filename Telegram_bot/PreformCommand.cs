using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using Telegram.SimpleJSON;
using System.Collections.Specialized;
using System.Net.Http;
using System.IO;
//Добавте токен!/Add the token!  В фаил/in file ->Settings.setting
//Добавте токен!/Add the token!  В фаил/in file ->Settings.setting
//Добавте токен!/Add the token!  В фаил/in file ->Settings.setting
namespace Telegram
{

    namespace SimpleJSON
    {
        public enum JSONBinaryTag
        {
            Array = 1,
            Class = 2,
            Value = 3,
            IntValue = 4,
            DoubleValue = 5,
            BoolValue = 6,
            FloatValue = 7,
        }

        public abstract class JSONNode
        {
            #region common interface

            public virtual void Add(string aKey, JSONNode aItem)
            {
            }

            public virtual JSONNode this[int aIndex] { get { return null; } set { } }

            public virtual JSONNode this[string aKey] { get { return null; } set { } }

            public virtual string Value { get { return ""; } set { } }

            public virtual int Count { get { return 0; } }

            public virtual void Add(JSONNode aItem)
            {
                Add("", aItem);
            }

            public virtual JSONNode Remove(string aKey)
            {
                return null;
            }

            public virtual JSONNode Remove(int aIndex)
            {
                return null;
            }

            public virtual JSONNode Remove(JSONNode aNode)
            {
                return aNode;
            }

            public virtual IEnumerable<JSONNode> Children
            {
                get
                {
                    yield break;
                }
            }

            public IEnumerable<JSONNode> DeepChildren
            {
                get
                {
                    foreach (var C in Children)
                        foreach (var D in C.DeepChildren)
                            yield return D;
                }
            }

            public override string ToString()
            {
                return "JSONNode";
            }

            public virtual string ToString(string aPrefix)
            {
                return "JSONNode";
            }

            public abstract string ToJSON(int prefix);

            #endregion common interface

            #region typecasting properties

            public virtual JSONBinaryTag Tag { get; set; }

            public virtual int AsInt
            {
                get
                {
                    int v = 0;
                    if (int.TryParse(Value, out v))
                        return v;
                    return 0;
                }
                set
                {
                    Value = value.ToString();
                    Tag = JSONBinaryTag.IntValue;
                }
            }

            public virtual float AsFloat
            {
                get
                {
                    float v = 0.0f;
                    if (float.TryParse(Value, out v))
                        return v;
                    return 0.0f;
                }
                set
                {
                    Value = value.ToString();
                    Tag = JSONBinaryTag.FloatValue;
                }
            }

            public virtual double AsDouble
            {
                get
                {
                    double v = 0.0;
                    if (double.TryParse(Value, out v))
                        return v;
                    return 0.0;
                }
                set
                {
                    Value = value.ToString();
                    Tag = JSONBinaryTag.DoubleValue;

                }
            }

            public virtual bool AsBool
            {
                get
                {
                    bool v = false;
                    if (bool.TryParse(Value, out v))
                        return v;
                    return !string.IsNullOrEmpty(Value);
                }
                set
                {
                    Value = (value) ? "true" : "false";
                    Tag = JSONBinaryTag.BoolValue;

                }
            }

            public virtual JSONArray AsArray
            {
                get
                {
                    return this as JSONArray;
                }
            }

            public virtual JSONClass AsObject
            {
                get
                {
                    return this as JSONClass;
                }
            }


            #endregion typecasting properties

            #region operators

            public static implicit operator JSONNode(string s)
            {
                return new JSONData(s);
            }

            public static implicit operator string(JSONNode d)
            {
                return (d == null) ? null : d.Value;
            }

            public static bool operator ==(JSONNode a, object b)
            {
                if (b == null && a is JSONLazyCreator)
                    return true;
                return System.Object.ReferenceEquals(a, b);
            }

            public static bool operator !=(JSONNode a, object b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return System.Object.ReferenceEquals(this, obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }


            #endregion operators

            internal static string Escape(string aText)
            {
                string result = "";
                foreach (char c in aText)
                {
                    switch (c)
                    {
                        case '\\':
                            result += "\\\\";
                            break;
                        case '\"':
                            result += "\\\"";
                            break;
                        case '\n':
                            result += "\\n";
                            break;
                        case '\r':
                            result += "\\r";
                            break;
                        case '\t':
                            result += "\\t";
                            break;
                        case '\b':
                            result += "\\b";
                            break;
                        case '\f':
                            result += "\\f";
                            break;
                        default:
                            result += c;
                            break;
                    }
                }
                return result;
            }

            static JSONData Numberize(string token)
            {
                bool flag = false;
                int integer = 0;
                double real = 0;

                if (int.TryParse(token, out integer))
                {
                    return new JSONData(integer);
                }

                if (double.TryParse(token, out real))
                {
                    return new JSONData(real);
                }

                if (bool.TryParse(token, out flag))
                {
                    return new JSONData(flag);
                }

                throw new NotImplementedException(token);
            }

            static void AddElement(JSONNode ctx, string token, string tokenName, bool tokenIsString)
            {
                if (tokenIsString)
                {
                    if (ctx is JSONArray)
                        ctx.Add(token);
                    else
                        ctx.Add(tokenName, token); // assume dictionary/object
                }
                else {
                    JSONData number = Numberize(token);
                    if (ctx is JSONArray)
                        ctx.Add(number);
                    else
                        ctx.Add(tokenName, number);

                }
            }

            public static JSONNode Parse(string aJSON)
            {
                Stack<JSONNode> stack = new Stack<JSONNode>();
                JSONNode ctx = null;
                int i = 0;
                string Token = "";
                string TokenName = "";
                bool QuoteMode = false;
                bool TokenIsString = false;
                while (i < aJSON.Length)
                {
                    switch (aJSON[i])
                    {
                        case '{':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            stack.Push(new JSONClass());
                            if (ctx != null)
                            {
                                TokenName = TokenName.Trim();
                                if (ctx is JSONArray)
                                    ctx.Add(stack.Peek());
                                else if (TokenName != "")
                                    ctx.Add(TokenName, stack.Peek());
                            }
                            TokenName = "";
                            Token = "";
                            ctx = stack.Peek();
                            break;

                        case '[':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }

                            stack.Push(new JSONArray());
                            if (ctx != null)
                            {
                                TokenName = TokenName.Trim();

                                if (ctx is JSONArray)
                                    ctx.Add(stack.Peek());
                                else if (TokenName != "")
                                    ctx.Add(TokenName, stack.Peek());
                            }
                            TokenName = "";
                            Token = "";
                            ctx = stack.Peek();
                            break;

                        case '}':
                        case ']':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            if (stack.Count == 0)
                                throw new Exception("JSON Parse: Too many closing brackets");

                            stack.Pop();
                            if (Token != "")
                            {
                                TokenName = TokenName.Trim();
                                /*
                                if (ctx is JSONArray)
                                    ctx.Add (Token);
                                else if (TokenName != "")
                                    ctx.Add (TokenName, Token);
                                    */
                                AddElement(ctx, Token, TokenName, TokenIsString);
                                TokenIsString = false;
                            }
                            TokenName = "";
                            Token = "";
                            if (stack.Count > 0)
                                ctx = stack.Peek();
                            break;

                        case ':':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            TokenName = Token;
                            Token = "";
                            TokenIsString = false;
                            break;

                        case '"':
                            QuoteMode ^= true;
                            TokenIsString = QuoteMode == true ? true : TokenIsString;
                            break;

                        case ',':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            if (Token != "")
                            {
                                /*
                                if (ctx is JSONArray) {
                                    ctx.Add (Token);
                                } else if (TokenName != "") {
                                    ctx.Add (TokenName, Token);
                                }
                                */
                                AddElement(ctx, Token, TokenName, TokenIsString);
                                TokenIsString = false;

                            }
                            TokenName = "";
                            Token = "";
                            TokenIsString = false;
                            break;

                        case '\r':
                        case '\n':
                            break;

                        case ' ':
                        case '\t':
                            if (QuoteMode)
                                Token += aJSON[i];
                            break;

                        case '\\':
                            ++i;
                            if (QuoteMode)
                            {
                                char C = aJSON[i];
                                switch (C)
                                {
                                    case 't':
                                        Token += '\t';
                                        break;
                                    case 'r':
                                        Token += '\r';
                                        break;
                                    case 'n':
                                        Token += '\n';
                                        break;
                                    case 'b':
                                        Token += '\b';
                                        break;
                                    case 'f':
                                        Token += '\f';
                                        break;
                                    case 'u':
                                        {
                                            string s = aJSON.Substring(i + 1, 4);
                                            Token += (char)int.Parse(
                                                s,
                                                System.Globalization.NumberStyles.AllowHexSpecifier);
                                            i += 4;
                                            break;
                                        }
                                    default:
                                        Token += C;
                                        break;
                                }
                            }
                            break;

                        default:
                            Token += aJSON[i];
                            break;
                    }
                    ++i;
                }
                if (QuoteMode)
                {
                    throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
                }
                return ctx;
            }

            public virtual void Serialize(System.IO.BinaryWriter aWriter)
            {
            }

            public void SaveToStream(System.IO.Stream aData)
            {
                var W = new System.IO.BinaryWriter(aData);
                Serialize(W);
            }

#if USE_SharpZipLib
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
			{
				gzipOut.IsStreamOwner = false;
				SaveToStream(gzipOut);
				gzipOut.Close();
			}
		}
 
		public void SaveToCompressedFile(string aFileName)
		{
 
#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToCompressedStream(F);
			}
 
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
		}
		public string SaveToCompressedBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToCompressedStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}
 
#else
            public void SaveToCompressedStream(System.IO.Stream aData)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }

            public void SaveToCompressedFile(string aFileName)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }

            public string SaveToCompressedBase64()
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
#endif

            public void SaveToFile(string aFileName)
            {
#if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            using (var F = System.IO.File.OpenWrite(aFileName))
            {
                SaveToStream(F);
            }
#else
                throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }

            public string SaveToBase64()
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    SaveToStream(stream);
                    stream.Position = 0;
                    return System.Convert.ToBase64String(stream.ToArray());
                }
            }

            public static JSONNode Deserialize(System.IO.BinaryReader aReader)
            {
                JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
                switch (type)
                {
                    case JSONBinaryTag.Array:
                        {
                            int count = aReader.ReadInt32();
                            JSONArray tmp = new JSONArray();
                            for (int i = 0; i < count; i++)
                                tmp.Add(Deserialize(aReader));
                            return tmp;
                        }
                    case JSONBinaryTag.Class:
                        {
                            int count = aReader.ReadInt32();
                            JSONClass tmp = new JSONClass();
                            for (int i = 0; i < count; i++)
                            {
                                string key = aReader.ReadString();
                                var val = Deserialize(aReader);
                                tmp.Add(key, val);
                            }
                            return tmp;
                        }
                    case JSONBinaryTag.Value:
                        {
                            return new JSONData(aReader.ReadString());
                        }
                    case JSONBinaryTag.IntValue:
                        {
                            return new JSONData(aReader.ReadInt32());
                        }
                    case JSONBinaryTag.DoubleValue:
                        {
                            return new JSONData(aReader.ReadDouble());
                        }
                    case JSONBinaryTag.BoolValue:
                        {
                            return new JSONData(aReader.ReadBoolean());
                        }
                    case JSONBinaryTag.FloatValue:
                        {
                            return new JSONData(aReader.ReadSingle());
                        }

                    default:
                        {
                            throw new Exception("Error deserializing JSON. Unknown tag: " + type);
                        }
                }
            }

#if USE_SharpZipLib
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
			return LoadFromStream(zin);
		}
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
#if USE_FileIO
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromCompressedStream(F);
			}
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromCompressedStream(stream);
		}
#else
            public static JSONNode LoadFromCompressedFile(string aFileName)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }

            public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }

            public static JSONNode LoadFromCompressedBase64(string aBase64)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
#endif

            public static JSONNode LoadFromStream(System.IO.Stream aData)
            {
                using (var R = new System.IO.BinaryReader(aData))
                {
                    return Deserialize(R);
                }
            }

            public static JSONNode LoadFromFile(string aFileName)
            {
#if USE_FileIO
            using (var F = System.IO.File.OpenRead(aFileName))
            {
                return LoadFromStream(F);
            }
#else
                throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }

            public static JSONNode LoadFromBase64(string aBase64)
            {
                var tmp = System.Convert.FromBase64String(aBase64);
                var stream = new System.IO.MemoryStream(tmp);
                stream.Position = 0;
                return LoadFromStream(stream);
            }
        }
        // End of JSONNode

        public class JSONArray : JSONNode, IEnumerable
        {
            private List<JSONNode> m_List = new List<JSONNode>();

            public override JSONNode this[int aIndex]
            {
                get
                {
                    if (aIndex < 0 || aIndex >= m_List.Count)
                        return new JSONLazyCreator(this);
                    return m_List[aIndex];
                }
                set
                {
                    if (aIndex < 0 || aIndex >= m_List.Count)
                        m_List.Add(value);
                    else
                        m_List[aIndex] = value;
                }
            }

            public override JSONNode this[string aKey]
            {
                get { return new JSONLazyCreator(this); }
                set { m_List.Add(value); }
            }

            public override int Count
            {
                get { return m_List.Count; }
            }

            public override void Add(string aKey, JSONNode aItem)
            {
                m_List.Add(aItem);
            }

            public override JSONNode Remove(int aIndex)
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return null;
                JSONNode tmp = m_List[aIndex];
                m_List.RemoveAt(aIndex);
                return tmp;
            }

            public override JSONNode Remove(JSONNode aNode)
            {
                m_List.Remove(aNode);
                return aNode;
            }

            public override IEnumerable<JSONNode> Children
            {
                get
                {
                    foreach (JSONNode N in m_List)
                        yield return N;
                }
            }

            public IEnumerator GetEnumerator()
            {
                foreach (JSONNode N in m_List)
                    yield return N;
            }

            public override string ToString()
            {
                string result = "[ ";
                foreach (JSONNode N in m_List)
                {
                    if (result.Length > 2)
                        result += ", ";
                    result += N.ToString();
                }
                result += " ]";
                return result;
            }

            public override string ToString(string aPrefix)
            {
                string result = "[ ";
                foreach (JSONNode N in m_List)
                {
                    if (result.Length > 3)
                        result += ", ";
                    result += "\n" + aPrefix + "   ";
                    result += N.ToString(aPrefix + "   ");
                }
                result += "\n" + aPrefix + "]";
                return result;
            }

            public override string ToJSON(int prefix)
            {
                string s = new string(' ', (prefix + 1) * 2);
                string ret = "[ ";
                foreach (JSONNode n in m_List)
                {
                    if (ret.Length > 3)
                        ret += ", ";
                    ret += "\n" + s;
                    ret += n.ToJSON(prefix + 1);

                }
                ret += "\n" + s + "]";
                return ret;
            }

            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                aWriter.Write((byte)JSONBinaryTag.Array);
                aWriter.Write(m_List.Count);
                for (int i = 0; i < m_List.Count; i++)
                {
                    m_List[i].Serialize(aWriter);
                }
            }
        }
        // End of JSONArray

        public class JSONClass : JSONNode, IEnumerable
        {
            private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

            public override JSONNode this[string aKey]
            {
                get
                {
                    if (m_Dict.ContainsKey(aKey))
                        return m_Dict[aKey];
                    else
                        return new JSONLazyCreator(this, aKey);
                }
                set
                {
                    if (m_Dict.ContainsKey(aKey))
                        m_Dict[aKey] = value;
                    else
                        m_Dict.Add(aKey, value);
                }
            }

            public override JSONNode this[int aIndex]
            {
                get
                {
                    if (aIndex < 0 || aIndex >= m_Dict.Count)
                        return null;
                    return m_Dict.ElementAt(aIndex).Value;
                }
                set
                {
                    if (aIndex < 0 || aIndex >= m_Dict.Count)
                        return;
                    string key = m_Dict.ElementAt(aIndex).Key;
                    m_Dict[key] = value;
                }
            }

            public override int Count
            {
                get { return m_Dict.Count; }
            }


            public override void Add(string aKey, JSONNode aItem)
            {
                if (!string.IsNullOrEmpty(aKey))
                {
                    if (m_Dict.ContainsKey(aKey))
                        m_Dict[aKey] = aItem;
                    else
                        m_Dict.Add(aKey, aItem);
                }
                else
                    m_Dict.Add(Guid.NewGuid().ToString(), aItem);
            }

            public override JSONNode Remove(string aKey)
            {
                if (!m_Dict.ContainsKey(aKey))
                    return null;
                JSONNode tmp = m_Dict[aKey];
                m_Dict.Remove(aKey);
                return tmp;
            }

            public override JSONNode Remove(int aIndex)
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;
                var item = m_Dict.ElementAt(aIndex);
                m_Dict.Remove(item.Key);
                return item.Value;
            }

            public override JSONNode Remove(JSONNode aNode)
            {
                try
                {
                    var item = m_Dict.Where(k => k.Value == aNode).First();
                    m_Dict.Remove(item.Key);
                    return aNode;
                }
                catch
                {
                    return null;
                }
            }

            public override IEnumerable<JSONNode> Children
            {
                get
                {
                    foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                        yield return N.Value;
                }
            }

            public IEnumerator GetEnumerator()
            {
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                    yield return N;
            }

            public override string ToString()
            {
                string result = "{";
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                {
                    if (result.Length > 2)
                        result += ", ";
                    result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
                }
                result += "}";
                return result;
            }

            public override string ToString(string aPrefix)
            {
                string result = "{ ";
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                {
                    if (result.Length > 3)
                        result += ", ";
                    result += "\n" + aPrefix + "   ";
                    result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix + "   ");
                }
                result += "\n" + aPrefix + "}";
                return result;
            }

            public override string ToJSON(int prefix)
            {
                string s = new string(' ', (prefix + 1) * 2);
                string ret = "{ ";
                foreach (KeyValuePair<string, JSONNode> n in m_Dict)
                {
                    if (ret.Length > 3)
                        ret += ", ";
                    ret += "\n" + s;
                    ret += string.Format("\"{0}\": {1}", n.Key, n.Value.ToJSON(prefix + 1));
                }
                ret += "\n" + s + "}";
                return ret;
            }

            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                aWriter.Write((byte)JSONBinaryTag.Class);
                aWriter.Write(m_Dict.Count);
                foreach (string K in m_Dict.Keys)
                {
                    aWriter.Write(K);
                    m_Dict[K].Serialize(aWriter);
                }
            }
        }
        // End of JSONClass

        public class JSONData : JSONNode
        {
            private string m_Data;


            public override string Value
            {
                get { return m_Data; }
                set
                {
                    m_Data = value;
                    Tag = JSONBinaryTag.Value;
                }
            }

            public JSONData(string aData)
            {
                m_Data = aData;
                Tag = JSONBinaryTag.Value;
            }

            public JSONData(float aData)
            {
                AsFloat = aData;
            }

            public JSONData(double aData)
            {
                AsDouble = aData;
            }

            public JSONData(bool aData)
            {
                AsBool = aData;
            }

            public JSONData(int aData)
            {
                AsInt = aData;
            }

            public override string ToString()
            {
                return "\"" + Escape(m_Data) + "\"";
            }

            public override string ToString(string aPrefix)
            {
                return "\"" + Escape(m_Data) + "\"";
            }

            public override string ToJSON(int prefix)
            {
                switch (Tag)
                {
                    case JSONBinaryTag.DoubleValue:
                    case JSONBinaryTag.FloatValue:
                    case JSONBinaryTag.IntValue:
                        return m_Data;
                    case JSONBinaryTag.Value:
                        return string.Format("\"{0}\"", Escape(m_Data));
                    default:
                        throw new NotSupportedException("This shouldn't be here: " + Tag.ToString());
                }
            }

            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                var tmp = new JSONData("");

                tmp.AsInt = AsInt;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.IntValue);
                    aWriter.Write(AsInt);
                    return;
                }
                tmp.AsFloat = AsFloat;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.FloatValue);
                    aWriter.Write(AsFloat);
                    return;
                }
                tmp.AsDouble = AsDouble;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.DoubleValue);
                    aWriter.Write(AsDouble);
                    return;
                }

                tmp.AsBool = AsBool;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.BoolValue);
                    aWriter.Write(AsBool);
                    return;
                }
                aWriter.Write((byte)JSONBinaryTag.Value);
                aWriter.Write(m_Data);
            }
        }
        // End of JSONData

        internal class JSONLazyCreator : JSONNode
        {
            private JSONNode m_Node = null;
            private string m_Key = null;

            public JSONLazyCreator(JSONNode aNode)
            {
                m_Node = aNode;
                m_Key = null;
            }

            public JSONLazyCreator(JSONNode aNode, string aKey)
            {
                m_Node = aNode;
                m_Key = aKey;
            }

            private void Set(JSONNode aVal)
            {
                if (m_Key == null)
                {
                    m_Node.Add(aVal);
                }
                else {
                    m_Node.Add(m_Key, aVal);
                }
                m_Node = null; // Be GC friendly.
            }

            public override JSONNode this[int aIndex]
            {
                get
                {
                    return new JSONLazyCreator(this);
                }
                set
                {
                    var tmp = new JSONArray();
                    tmp.Add(value);
                    Set(tmp);
                }
            }

            public override JSONNode this[string aKey]
            {
                get
                {
                    return new JSONLazyCreator(this, aKey);
                }
                set
                {
                    var tmp = new JSONClass();
                    tmp.Add(aKey, value);
                    Set(tmp);
                }
            }

            public override void Add(JSONNode aItem)
            {
                var tmp = new JSONArray();
                tmp.Add(aItem);
                Set(tmp);
            }

            public override void Add(string aKey, JSONNode aItem)
            {
                var tmp = new JSONClass();
                tmp.Add(aKey, aItem);
                Set(tmp);
            }

            public static bool operator ==(JSONLazyCreator a, object b)
            {
                if (b == null)
                    return true;
                return System.Object.ReferenceEquals(a, b);
            }

            public static bool operator !=(JSONLazyCreator a, object b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return true;
                return System.Object.ReferenceEquals(this, obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return "";
            }

            public override string ToString(string aPrefix)
            {
                return "";
            }

            public override string ToJSON(int prefix)
            {
                return "";
            }

            public override int AsInt
            {
                get
                {
                    JSONData tmp = new JSONData(0);
                    Set(tmp);
                    return 0;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }

            public override float AsFloat
            {
                get
                {
                    JSONData tmp = new JSONData(0.0f);
                    Set(tmp);
                    return 0.0f;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }

            public override double AsDouble
            {
                get
                {
                    JSONData tmp = new JSONData(0.0);
                    Set(tmp);
                    return 0.0;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }

            public override bool AsBool
            {
                get
                {
                    JSONData tmp = new JSONData(false);
                    Set(tmp);
                    return false;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }

            public override JSONArray AsArray
            {
                get
                {
                    JSONArray tmp = new JSONArray();
                    Set(tmp);
                    return tmp;
                }
            }

            public override JSONClass AsObject
            {
                get
                {
                    JSONClass tmp = new JSONClass();
                    Set(tmp);
                    return tmp;
                }
            }
        }
        // End of JSONLazyCreator

        public static class JSON
        {
            public static JSONNode Parse(string aJSON)
            {
                return JSONNode.Parse(aJSON);
            }
        }
    }

    namespace Request
    {
        #region DefaultClass
        public class User
        {
            public int id;
            public string first_name;
            public string last_name;
            public string username;
        }
        public class Chat
        {
            public int id;
            public string type;
            public string title;
            public string username;
            public string first_name;
            public string last_name;

        }
        public class PhotoSize
        {
            public string file_id;
            public int width;
            public int height;
            public int file_size;
        }
        #endregion
        #region Делгаты
        public delegate void ResponseText(object sendr, MessageText e);
        public delegate void ResponseSticker(object sendr, MessageSticker e);
        public delegate void ResponsePhoto(object sendr, MessagePhoto e);
        public delegate void ResponseVideo(object sendr, MessageVideo e);
        public delegate void ResponseDocument(object sendr, MessageDocument e);
        public delegate void ResponseLocation(object sendr, MessageLocation e);
        public delegate void ResponseContact(object sendr, MessageContact e);
        public delegate void ResponseVoice(object sendr, MessageVoice e);
        #endregion
        #region Классы Типов
        public class MessageText : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public string text;
        }
        public class MessageSticker : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public string file_id;
            public int width;
            public int height;
            public PhotoSize thumb = new PhotoSize();
            public string emoji;
            public int file_size;
        }
        public class MessagePhoto : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public List<PhotoSize> photo = new List<PhotoSize>();
            public string caption;
        }
        public class MessageVideo : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public string file_id;
            public int width;
            public int height;
            public int duration;
            public PhotoSize thumb = new PhotoSize();
            public string mime_type;
            public int file_size;
        }
        public class MessageDocument : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public string file_id;
            public PhotoSize thumb = new PhotoSize();
            public string file_name;
            public string mime_type;
            public int file_size;
        }
        public class MessageLocation : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public float longitude;
            public float latitude;
        }
        public class MessageContact : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public string phone_number;
            public string first_name;
            public string last_name;
            public int user_id;
        }
        public class MessageVoice : EventArgs
        {
            #region DefaulParameters
            public int message_id;
            public User from = new User();
            public Chat chat = new Chat();
            public int date;
            #endregion
            public string file_id;
            public int duration;
            public string mime_type;
            public int file_size;
        }
        #endregion


        public class TelegramRequest
        {
            public string _token;
            public TelegramRequest(string Token)
            {
                _token = Token;
            }
            int LastUpdateID = 0;
            //События
            public event ResponseText MessageText;
            public event ResponseSticker MessageSticker;
            public event ResponsePhoto MessagePhoto;
            public event ResponseVideo MessageVideo;
            public event ResponseDocument MessageDocument;
            public event ResponseLocation MessageLocation;
            public event ResponseContact MessageContact;
            public event ResponseVoice MessageVoice;

            public void GetUpdates()
            {
                while (true)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        string response = webClient.DownloadString("https://api.telegram.org/bot" + _token + "/getupdates?offset=" + (LastUpdateID + 1));
                        if (response.Length <= 23)
                            continue;
                        var N = JSON.Parse(response);
                        foreach (JSONNode r in N["result"].AsArray)
                        {
                            string _type = r["message"].ToString();
                            _type = WhatsType(_type).Replace("\"", "");
                            LastUpdateID = r["update_id"].AsInt;
                            #region SWITCH
                            switch (_type)
                            {
                                case "text":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили текстовое сообщение");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessageText(r);
                                        break;
                                    }
                                case "sticker":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили cтикер");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessageSticker(r);
                                        break;
                                    }
                                case "photo":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили фотографию");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessagePhoto(r);
                                        break;
                                    }
                                case "video":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили видеозапись");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessageVideo(r);
                                        break;
                                    }
                                case "document":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили документ");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessageDocument(r);
                                        break;
                                    }
                                case "location":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили локацию");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessageLocation(r);
                                        break;
                                    }
                                case "contact":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили контакт");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessageContact(r);
                                        break;
                                    }
                                case "voice":
                                    {
                                        try
                                        {
                                            MessageText.Method.Name.ToString();
                                            Notification();
                                            Console.WriteLine("Мы получили голосовую запись");
                                            Notification(false);
                                        }
                                        catch (Exception)
                                        { break; }
                                        GetMessageVoice(r);
                                        break;
                                    }
                            }
                            #endregion
                        }
                    }
                }
            }
            private string WhatsType(string JSON)
            {
                string[] Type = { "text", "sticker", "photo", "video", "document", "location", "contact", "voice" };
                for (int i = 0; i < Type.Length; i++)
                {
                    try { JSON = JSON.Remove(0, JSON.LastIndexOf("\"" + Type[i] + "\"")); }
                    catch (Exception){continue;}
                    JSON = JSON.Remove(JSON.IndexOf(":"));
                    break;
                }
                return JSON;
            }
        
            private void Notification(bool on = true)
            {
                if (on)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            #region Method
            private void GetMessageText(JSONNode r)
            {
                MessageText message = new MessageText();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //text
                message.text = r["message"]["text"];
                //Event
                MessageText(this, message);
            }
            private void GetMessageSticker(JSONNode r)
            {
                MessageSticker message = new MessageSticker();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //sticker
                message.width = r["message"]["sticker"]["width"].AsInt;
                message.height = r["message"]["sticker"]["height"].AsInt;
                message.emoji = r["message"]["sticker"]["height"];
                //thumb
                message.thumb.file_id = r["message"]["sticker"]["thumb"]["file_id"];
                message.thumb.file_size = r["message"]["sticker"]["thumb"]["file_size"].AsInt;
                message.thumb.width = r["message"]["sticker"]["thumb"]["width"].AsInt;
                message.thumb.height = r["message"]["sticker"]["thumb"]["height"].AsInt;
                //ene thumb
                message.file_id = r["message"]["sticker"]["file_id"];
                message.file_size = r["message"]["sticker"]["file_size"].AsInt;
                //Event
                MessageSticker(this, message);
            }
            private void GetMessagePhoto(JSONNode r)
            {
                MessagePhoto message = new MessagePhoto();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //sticker
                for (int i = 0; i < r["message"]["photo"].Count; i++)
                {
                    message.photo.Add(new PhotoSize
                    {
                        file_id = r["message"]["photo"][i]["file_id"],
                        file_size = r["message"]["photo"][i]["file_size"].AsInt,
                        height = r["message"]["photo"][i]["height"].AsInt,
                        width = r["message"]["photo"][i]["width"].AsInt
                    });
                }
                message.caption = r["message"]["caption"];
                //Event
                MessagePhoto(this, message);
            }
            private void GetMessageVideo(JSONNode r)
            {
                MessageVideo message = new MessageVideo();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //video
                message.file_id = r["message"]["video"]["file_id"];
                message.duration = r["message"]["video"]["duration"].AsInt;
                message.width = r["message"]["video"]["width"].AsInt;
                message.height = r["message"]["video"]["height"].AsInt;
                //video-thum
                message.thumb.file_id = r["message"]["video"]["thumb"]["file_id"];
                message.thumb.file_size = r["message"]["video"]["thumb"]["file_size"].AsInt;
                message.thumb.width = r["message"]["video"]["thumb"]["width"].AsInt;
                message.thumb.height = r["message"]["video"]["thumb"]["height"].AsInt;
                //thumb end
                message.mime_type = r["message"]["video"]["mime_type"];
                message.file_size = r["message"]["video"]["file_size"].AsInt;
                //Event
                MessageVideo(this, message);
            }
            private void GetMessageDocument(JSONNode r)
            {
                MessageDocument message = new MessageDocument();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //document
                message.file_id = r["message"]["document"]["file_id"];
                message.file_name = r["message"]["document"]["file_name"];
                message.mime_type = r["message"]["document"]["mime_type"];
                message.file_size = r["message"]["document"]["file_size"].AsInt;
                //document-thumb
                message.thumb.file_id = r["message"]["document"]["thumb"]["file_id"];
                message.thumb.file_size = r["message"]["document"]["thumb"]["file_size"].AsInt;
                message.thumb.width = r["message"]["document"]["thumb"]["width"].AsInt;
                message.thumb.height = r["message"]["document"]["thumb"]["height"].AsInt;
                //thumb end
                //Event
                MessageDocument(this, message);
            }
            private void GetMessageLocation(JSONNode r)
            {
                MessageLocation message = new MessageLocation();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //location
                message.longitude = r["message"]["location"]["longitude"].AsFloat;
                message.latitude = r["message"]["location"]["latitude"].AsFloat;
                //Event
                MessageLocation(this, message);
            }
            private void GetMessageContact(JSONNode r)
            {
                MessageContact message = new MessageContact();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //contact
                message.phone_number = r["message"]["contact"]["phone_number"];
                message.first_name = r["message"]["contact"]["first_name"];
                message.last_name = r["message"]["contact"]["last_name"];
                message.user_id = r["message"]["contact"]["user_id"].AsInt;
                //Event
                MessageContact(this, message);
            }
            private void GetMessageVoice(JSONNode r)
            {
                MessageVoice message = new MessageVoice();
                #region GetDefaultInformation
                //message_id
                message.message_id = r["message"]["message_id"].AsInt;
                //from
                message.from.id = r["message"]["from"]["id"].AsInt;
                message.from.first_name = r["message"]["from"]["first_name"];
                message.from.last_name = r["message"]["from"]["last_name"];
                message.from.username = r["message"]["from"]["username"];
                //chat
                message.chat.id = r["message"]["chat"]["id"].AsInt;
                message.chat.first_name = r["message"]["chat"]["first_name"];
                message.chat.last_name = r["message"]["chat"]["last_name"];
                message.chat.username = r["message"]["chat"]["username"];
                message.chat.type = r["message"]["chat"]["type"];
                message.chat.title = r["message"]["chat"]["title"];
                //date
                message.date = r["message"]["date"].AsInt;
                #endregion
                //voice
                message.file_id = r["message"]["voice"]["file_id"];
                message.duration = r["message"]["voice"]["duration"].AsInt;
                message.mime_type = r["message"]["voice"]["mime_type"];
                message.file_size = r["message"]["voice"]["file_size"].AsInt;
                //Event
                MessageVoice(this, message);
            }
            #endregion
        }
    }

    class Method
    {
        string _token;
        string LINK = "https://api.telegram.org/bot";
        public Method(string Token)
        {
            _token = Token;
        }
        public string Getme()
        {
            using (WebClient webClient = new WebClient())
            {
                string response = webClient.DownloadString(LINK + _token + "/getMe");
                return response;
            }
        }

        public void SendMessage(string message,int ChatID)
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", ChatID.ToString());
                pars.Add("text", message);
                webClient.UploadValues(LINK + _token + "/sendMessage", pars);
            }
        }

        public void ForwardMessage(int fromChatID,int chatId,int messageID)
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatId.ToString());
                pars.Add("from_chat_id", fromChatID.ToString());
                pars.Add("message_id", messageID.ToString());
                webClient.UploadValues(LINK + _token + "/forwardMessage", pars);
            }
        }

        async public Task SendPhotoIputFile(int ChatID,string pathToPhoto,string catprion = "")
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string url = LINK + _token + "/sendPhoto";
                string fileName = pathToPhoto.Split('\\').Last();

                form.Add(new StringContent(ChatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(catprion.ToString(), Encoding.UTF8), "caption");
                using (FileStream fileStream = new FileStream(pathToPhoto,FileMode.Open,FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "photo", fileName);
                    using (HttpClient client = new HttpClient())
                        await client.PostAsync(url, form);
                }
            }

        }
        public void SendPhotoLink(int ChatID,string linkToPhoto,string caption="")
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", ChatID.ToString());
                pars.Add("photo", linkToPhoto);
                pars.Add("caption", caption);
                webClient.UploadValues(LINK + _token + "/sendPhoto", pars);
            }
        }

        async public Task SendAudioIputFile(int ChatID, string pathToAudio, string catprion = "", int duration= 0, string performer= "", string title="")
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string url = LINK + _token + "/sendAudio";
                string fileName = pathToAudio.Split('\\').Last();

                form.Add(new StringContent(ChatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(catprion.ToString(), Encoding.UTF8), "caption");
                form.Add(new StringContent(duration.ToString(), Encoding.UTF8), "duration");
                form.Add(new StringContent(performer.ToString(), Encoding.UTF8), "performer");
                form.Add(new StringContent(title.ToString(), Encoding.UTF8), "title");
                using (FileStream fileStream = new FileStream(pathToAudio, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "audio", fileName);
                    using (HttpClient client = new HttpClient())
                        await client.PostAsync(url, form);
                }
            }

        }
        public void SendAudioLink(int ChatID, string linkToAudio, string caption = "")
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", ChatID.ToString());
                pars.Add("audio", linkToAudio);
                pars.Add("caption", caption);
                webClient.UploadValues(LINK + _token + "/sendAudio", pars);
            }
        }

        async public Task SendDocumentIputFile(int ChatID, string pathToDocument, string catprion = "")
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string url = LINK + _token + "/sendDocument";
                string fileName = pathToDocument.Split('\\').Last();

                form.Add(new StringContent(ChatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(catprion.ToString(), Encoding.UTF8), "caption");
                using (FileStream fileStream = new FileStream(pathToDocument, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "document", fileName);
                    using (HttpClient client = new HttpClient())
                        await client.PostAsync(url, form);
                }
            }

        }
        public void SendDocumentoLink(int ChatID, string linkToDocument, string caption = "")
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", ChatID.ToString());
                pars.Add("document", linkToDocument);
                pars.Add("caption", caption);
                webClient.UploadValues(LINK + _token + "/sendDocument", pars);
            }
        }

        public void SendSticker(int chatID, string IDsticker)
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatID.ToString());
                pars.Add("sticker", IDsticker);
                webClient.UploadValues("https://api.telegram.org/bot" + _token + "/sendSticker", pars);
            }
        }

        async public Task SendVideoInputFile(int chatID, string pathToVideo, string caption = "")
        {
            using (var form = new MultipartFormDataContent())
            {
                string url = string.Format("https://api.telegram.org/bot{0}/sendVideo", _token);
                string fileName = pathToVideo.Split('\\').Last();

                form.Add(new StringContent(chatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(caption, Encoding.UTF8), "caption");

                using (FileStream fileStream = new FileStream(pathToVideo, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "video", fileName);
                    using (var client = new HttpClient())
                    {
                        await client.PostAsync(url, form);
                    }
                }
            }
        }
        public void SendVideoLink(int chatID, string linkToVideo, string caption = "")
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatID.ToString());
                pars.Add("video", linkToVideo);
                pars.Add("caption", caption);
                webClient.UploadValues("https://api.telegram.org/bot" + _token + "/sendVideo", pars);
            }
        }

        async public Task SendVoiceInputFile(int chatID, string pathToVoice, string caption = "", int duration = 0)
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string url = "https://api.telegram.org/bot" + _token + "/sendVoice";
                string fileName = pathToVoice.Split('\\').Last();

                form.Add(new StringContent(chatID.ToString(), Encoding.UTF8), "chat_id");
                using (FileStream fileStream = new FileStream(pathToVoice, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "voice", fileName);
                    form.Add(new StringContent(caption, Encoding.UTF8), "caption");
                    form.Add(new StringContent(duration.ToString(), Encoding.UTF8), "duration");
                    using (HttpClient client = new HttpClient())
                    {
                        await client.PostAsync(url, form);
                    }
                }
            }
        }
        public void SendVoiceLink(int chatID, string linkToAudio, string caption = "")
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatID.ToString());
                pars.Add("voice", linkToAudio);
                pars.Add("caption", caption);
                webClient.UploadValues("https://api.telegram.org/bot" + _token + "/sendVoice", pars);
            }
        }

        public void SendLocation(int chatID, float latitude, float longitude)
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatID.ToString());
                pars.Add("latitude", latitude.ToString());
                pars.Add("longitude", longitude.ToString());
                webClient.UploadValues("https://api.telegram.org/bot" + _token + "/SendLocation", pars);
            }
        }

        public void SendVenue(int chatID, float latitude, float longitude, string title, string address, string foursquare_id = "1")
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatID.ToString());
                pars.Add("latitude", latitude.ToString());
                pars.Add("longitude", longitude.ToString());
                pars.Add("title", title);
                pars.Add("address", address);
                pars.Add("foursquare_id", foursquare_id);
                webClient.UploadValues("https://api.telegram.org/bot" + _token + "/SendVenue", pars);
            }
        }

        public void SendContact(int chatID, string phone_number, string first_name, string last_name)
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatID.ToString());
                pars.Add("phone_number", phone_number);
                pars.Add("first_name", first_name);
                pars.Add("last_name", last_name);
                webClient.UploadValues("https://api.telegram.org/bot" + _token + "/SendContact", pars);
            }
        }

        public void SendChatAction(int chatID, ChatAction action)
        {
            using (WebClient webClient = new WebClient())
            {
                NameValueCollection pars = new NameValueCollection();
                pars.Add("chat_id", chatID.ToString());
                pars.Add("action", action.ToString());
                webClient.UploadValues("https://api.telegram.org/bot" + _token + "/sendChatAction", pars);
            }
        }
        public enum ChatAction
        {
            typing,
            upload_photo,
            record_video,
            upload_video,
            record_audio,
            upload_audio,
            upload_document,
            find_location
        }

        public string getUserProfilePhotos(int user_id, int offset, int limit=100)
        {
            using (WebClient webClient = new WebClient())
            {
                string response = webClient.DownloadString(LINK + _token + "/getUserProfilePhotos?user_id=" + user_id +"&limit=" + limit+ "&offset="+offset);
                JSONNode N = JSON.Parse(response);
                N = N["result"]["photos"].AsArray[0];
                string linkPhoto = N[N.Count - 1]["file_id"];
                return linkPhoto;
            }
        }

        public string[] getUserProfilePhotosAllTime(int user_id, int offset, int limit = 100)
        {
            using (WebClient webClient = new WebClient())
            {
                string response = webClient.DownloadString(LINK + _token + "/getUserProfilePhotos?user_id=" + user_id + "&limit=" + limit + "&offset=" + offset);
                JSONNode N = JSON.Parse(response);
                string[] linkPhoto = new string[N["result"]["total_count"].AsInt];
                int k = 0;
                foreach (JSONNode r in N["result"]["photos"].AsArray)
                {
                    linkPhoto[k] = r[r.Count - 1]["file_id"];
                    k++;
                }
                return linkPhoto;
            }
        }

        public string getFile(string file_id)
        {
            using (WebClient webClient = new WebClient())
            {
                string response = webClient.DownloadString(LINK + _token + "/getFile?file_id=" + file_id);
                JSONNode N = JSON.Parse(response);
                response = "https://api.telegram.org/file/bot" + _token + "/" + N["result"]["file_path"];
                return response;
            }
        }
    }
}
