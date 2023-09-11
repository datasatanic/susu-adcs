using System.Text.Json.Serialization;
using System.Xml.Serialization;
using MessagePack;
using YamlDotNet.Serialization;

namespace SerializationFormats;

[MessagePackObject]
public class Model
{
    [Key(0)] public string LongString { get; init; }
    [Key(1)] [XmlArray] public int[] IntArray { get; init; }
    [Key(2)] public InnerModel InnerModel { get; init; }
    [Key(3)] public double DoubleValue { get; init; }
    [Key(4)] public long LongValue { get; init; }

    // internal ImmutableDictionary<string, string> _Dictionary { get; init; }

    [XmlIgnore] [Key(6)] public Dictionary<string, string> Dictionary { get; set; }

    [JsonIgnore]
    [YamlIgnore]
    [IgnoreMember]
    public KeyVal[] Dict
    {
        get => Dictionary.Select(pair => new KeyVal(pair.Key, pair.Value)).ToArray();
        set => Dictionary = value.ToDictionary(val => val.Key, val => val.Val);
    }
}

public class KeyVal
{
    public string Key;
    public string Val;

    public KeyVal()
    {
    }

    public KeyVal(string key, string val)
    {
        Key = key;
        Val = val;
    }
}

[MessagePackObject]
public class InnerModel
{
    [Key(0)] public string InnerString { get; init; }
    [Key(1)] public int InnerInt { get; init; }
    [Key(2)] public double InnerDouble { get; init; }
}