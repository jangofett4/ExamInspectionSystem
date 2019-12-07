using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public enum ConfigValueType
    {
        String,
        Integer,
        Float,
        Boolean
    }

    public struct ConfigValue
    {
        public object Value;
        public ConfigValueType Type;

        public ConfigValue(object value, ConfigValueType type)
        {
            Value = value;
            Type = type;
        }

        public ConfigValue(string value) : this(value, ConfigValueType.String) { }
        public ConfigValue(int value) : this(value, ConfigValueType.Integer) { }
        public ConfigValue(float value) : this(value, ConfigValueType.Float) { }
        public ConfigValue(bool value) : this(value, ConfigValueType.Boolean) { }

        public T Cast<T>()
        {
            return (T)Value;
        }
    }

    public class BasicConfig
    {
        public Dictionary<string, ConfigValue> Values { get; set; }
        public string File { get; set; }

        public BasicConfig(string file)
        {
            File = file;
            Values = new Dictionary<string, ConfigValue>();
        }

        public bool Push(string key, object value, ConfigValueType type)
        {
            if (Values.ContainsKey(key))
                return false;
            Values.Add(key, new ConfigValue(value, type));
            return true;
        }

        public bool GetString(string key, out string value)
        {
            value = "";
            if (!Values.TryGetValue(key, out ConfigValue val)) return false;
            if (val.Type != ConfigValueType.String) return false;
            value = val.Cast<string>();
            return true;
        }

        public bool GetInteger(string key, out int value)
        {
            value = -1;
            if (!Values.TryGetValue(key, out ConfigValue val)) return false;
            if (val.Type != ConfigValueType.Integer) return false;
            value = val.Cast<int>();
            return true;
        }

        public bool GetFloat(string key, out float value)
        {
            value = -1;
            if (!Values.TryGetValue(key, out ConfigValue val)) return false;
            if (val.Type != ConfigValueType.Float) return false;
            value = val.Cast<float>();
            return true;
        }

        public bool GetBoolean(string key, out bool value)
        {
            value = false;
            if (!Values.TryGetValue(key, out ConfigValue val)) return false;
            if (val.Type != ConfigValueType.Boolean) return false;
            value = val.Cast<bool>();
            return true;
        }

        public bool If(string key, Action @if)
        {
            if (!GetBoolean(key, out bool val))
                return false;
            if (val)
                @if();
            return true;
        }

        public bool If(string key, Action @if, Action @else)
        {
            if (!GetBoolean(key, out bool val))
                return false;
            if (val)
                @if();
            else
                @else();
            return true;
        }

        public bool Read()
        {
            Values.Clear();
            var content = System.IO.File.ReadAllText(File, Encoding.UTF8);
            var split = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ln in split)
            {
                var line = ln.Trim();

                if (line.StartsWith("#")) continue; // Comment line
                if (string.IsNullOrWhiteSpace(line)) continue; // Empty line

                char c = '\0';
                int i = 0;
                string key = "";
                while (i < line.Length && (c = line[i++]) != '=')
                {
                    if (char.IsWhiteSpace(c)) continue;
                    key += c;
                }
                if (i == line.Length)
                    return false;
                ConfigValue? val = null;
                while (i < line.Length)
                {
                    c = line[i];
                    if (char.IsWhiteSpace(c)) { i++; continue; }
                    if (c == '"' || c == '\'')
                    {
                        char starter = c;
                        string str = "";
                        while (i < line.Length && (c = line[i++]) != starter)
                            str += c;
                        val = new ConfigValue(str);
                        break;
                    }
                    if (char.IsDigit(c))
                    {
                        string tok = "" + c;
                        bool f = false;
                        while (i < line.Length && (char.IsDigit(c = line[i++]) || c == '.' || c == ','))
                        {
                            if (c == '.' || c == ',')
                            {
                                if (f)
                                    continue; // excess '.' or ',' wont matter, first one considered true
                                f = true;
                                tok += ',';
                                continue;
                            }
                            tok += c;
                        }
                        if (f)
                            val = new ConfigValue(float.Parse(tok));
                        else
                            val = new ConfigValue(int.Parse(tok));
                        break;
                    }
                    if (char.ToLower(c) == 't' || char.ToLower(c) == 'f')
                    {
                        val = new ConfigValue(c == 't' ? true : false);
                        break;
                    }
                    break; // no values is read, something is wrong
                }
                if (!val.HasValue)
                    return false;
                Values.Add(key, val.Value);
            }
            return true;
        }

        public void Write()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var k in Values)
            {
                if (k.Value.Type == ConfigValueType.String)
                    sb.AppendLine($"{ k.Key } = '{ k.Value.Cast<string>() }'");
                else if (k.Value.Type == ConfigValueType.Integer)
                    sb.AppendLine($"{ k.Key } = { k.Value.Cast<int>().ToString() }");
                else
                    sb.AppendLine($"{ k.Key } = { k.Value.Cast<float>().ToString().Replace(',', '.') }");
            }
            System.IO.File.WriteAllText(File, sb.ToString(), Encoding.UTF8);
        }
    }
}
