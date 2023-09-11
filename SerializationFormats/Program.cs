using System.Text.Json;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using Faker;
using Google.Protobuf;
using MessagePack;
using SerializationFormats;
using SolTechnology.Avro;
using YamlDotNet.Serialization;
using DataModel = SerializationFormats.Model;
using ProtoModel = Model.Model;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

internal class Data
{
    public static XmlSerializer _xmlSerializer = new(typeof(DataModel));
    public static Serializer _yamlSerializer = new SerializerBuilder().Build();
    public static Deserializer _yamlDeserializer = new DeserializerBuilder().Build();

    public static DataModel data = new()
    {
        LongString = Lorem.Paragraph(),
        LongValue = Random.Shared.NextInt64(),
        DoubleValue = Random.Shared.NextDouble(),
        IntArray = Enumerable.Range(0, 20).Select(i => Random.Shared.Next()).ToArray(),
        InnerModel = new InnerModel
        {
            InnerString = Lorem.Sentence(),
            InnerDouble = Random.Shared.NextDouble(),
            InnerInt = Random.Shared.Next()
        },
        Dictionary = Enumerable.Range(0, 10)
            .Select(_ =>
            {
                return new KeyValuePair<string, string>(Identification.UsPassportNumber(),
                    Name.FullName());
            }).ToDictionary(pair => pair.Key, pair => pair.Value)
    };

    public static ProtoModel protoData = new()
    {
        LongString = data.LongString,
        DoubleValue = data.DoubleValue,
        LongValue = data.LongValue,
        InnerModel = new Model.InnerModel
        {
            InnerDouble = data.InnerModel.InnerDouble,
            InnerString = data.InnerModel.InnerString,
            InnerInt = data.InnerModel.InnerInt
        },
        Dictionary = { data.Dictionary.ToDictionary(pair => pair.Key, pair => pair.Value) },
        IntArray = { data.IntArray }
    };
}


[SimpleJob(RunStrategy.ColdStart)]
[MemoryDiagnoser]
public class Serialization
{
    [Benchmark(Baseline = true)]
    public string JSON()
    {
        return JsonSerializer.Serialize(Data.data);
    }

    [Benchmark]
    public string XML()
    {
        using (var writer = new StringWriter())
        {
            Data._xmlSerializer.Serialize(writer, Data.data);
            return writer.ToString();
        }
    }

    [Benchmark]
    public string YAML()
    {
        return Data._yamlSerializer.Serialize(Data.data);
    }


    [Benchmark]
    public byte[] MessagePack()
    {
        return MessagePackSerializer.Serialize(Data.data);
    }

    [Benchmark]
    public byte[] ProtoBuf()
    {
        var s = new MemoryStream();
        Data.protoData.WriteTo(s);
        return s.ToArray();
    }

    [Benchmark]
    public byte[] Avro()
    {
        return AvroConvert.Serialize(Data.data);
    }
}

[SimpleJob(RunStrategy.ColdStart)]
public class Deserialization
{
    public static string _json;
    private byte[] _avro;
    private byte[] _msgPack;
    private byte[] _pbf;
    private string _xml;
    private string _yaml;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var ser = new Serialization();
        _json = ser.JSON();
        _xml = ser.XML();
        _yaml = ser.YAML();
        _msgPack = ser.MessagePack();
        _pbf = ser.ProtoBuf();
        _avro = ser.Avro();
    }

    [Benchmark]
    public DataModel? JSON()
    {
        return JsonSerializer.Deserialize<DataModel>(_json);
    }

    [Benchmark]
    public DataModel? XML()
    {
        using var reader = new StringReader(_xml);
        return (DataModel?)Data._xmlSerializer.Deserialize(reader);
    }

    [Benchmark]
    public DataModel? YAML()
    {
        return Data._yamlDeserializer.Deserialize<DataModel>(_yaml);
    }

    [Benchmark]
    public DataModel MsgPack()
    {
        return MessagePackSerializer.Deserialize<DataModel>(_msgPack);
    }

    [Benchmark]
    public ProtoModel ProtoBuf()
    {
        return ProtoModel.Parser.ParseFrom(_pbf);
    }

    [Benchmark]
    public DataModel Avro()
    {
        return AvroConvert.Deserialize<DataModel>(_avro);
    }
}

[SimpleJob(RunStrategy.ColdStart)]
public class Full
{
    [Benchmark(Baseline = true)]
    public DataModel? JSON()
    {
        var s = JsonSerializer.Serialize(Data.data);
        return JsonSerializer.Deserialize<DataModel>(s);
    }

    [Benchmark]
    public DataModel? XML()
    {
        using var writer = new StringWriter();

        Data._xmlSerializer.Serialize(writer, Data.data);
        using var reader = new StringReader(writer.ToString());
        return (DataModel)Data._xmlSerializer.Deserialize(reader)!;
    }

    [Benchmark]
    public DataModel YAML()
    {
        var s = Data._yamlSerializer.Serialize(Data.data);
        return Data._yamlDeserializer.Deserialize<DataModel>(s);
    }


    [Benchmark]
    public DataModel MessagePack()
    {
        var bytes = MessagePackSerializer.Serialize(Data.data);
        return MessagePackSerializer.Deserialize<DataModel>(bytes);
    }

    [Benchmark]
    public ProtoModel ProtoBuf()
    {
        var s = new MemoryStream();
        Data.protoData.WriteTo(s);
        return ProtoModel.Parser.ParseFrom(s);
    }

    [Benchmark]
    public DataModel Avro()
    {
        var bytes = AvroConvert.Serialize(Data.data);
        return AvroConvert.Deserialize<DataModel>(bytes);
    }
}