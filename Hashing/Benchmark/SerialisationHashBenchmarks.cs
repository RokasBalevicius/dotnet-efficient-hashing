using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using Hashing.Data;
using Hashing.HashCalculator;
using K4os.Hash.xxHash;
using ProtoBuf;
using Standart.Hash.xxHash;

namespace Hashing.Benchmark;

[MemoryDiagnoser]
public class SerialisationHashBenchmarks
{
    private readonly TestObject _testObject;

    private readonly JsonSerializerOptions _jsonOptions;

    private readonly Func<TestObject, long> _customHashCalculator;
    private readonly Func<TestObject, XXH64.State, ulong> _customXXHash64SerialisationFunction;
    
    public SerialisationHashBenchmarks()
    {
        _testObject = TestDataGenerator.GenerateTestSampleWithLowFillLevel();
        //_testObject = TestDataGenerator.GenerateTestSampleWithMediumFillLevel();
        //_testObject = TestDataGenerator.GenerateTestSampleWithHighFillLevel();

        var builder = new CustomHashCalculatorBuilder();
        _customHashCalculator = builder.BuildHashCalculator<TestObject>();
        
        var streamHashBuilder = new XXHash64HasherBuilder();
        _customXXHash64SerialisationFunction = streamHashBuilder.BuildSerialisationFunction<TestObject>();

        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
    
    [Benchmark]
    public void StandardHashXXHash64_Json()
    {
        var _ = xxHash64.ComputeHash(JsonSerializer.Serialize(_testObject));
    }
    
    [Benchmark]
    public void StandardHashXXHash64_Json_IgnoreNulls()
    {
        var _ = xxHash64.ComputeHash(JsonSerializer.Serialize(_testObject, _jsonOptions));
    }
    
    [Benchmark]
    public void StandardHashXXHash64_JsonStream()
    {
        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, _testObject);
        var _ = xxHash64.ComputeHash(stream);
    }
    
    [Benchmark]
    public void StandardHashXXHash64_JsonStream_IgnoreNulls()
    {
        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, _testObject, _jsonOptions);
        var _ = xxHash64.ComputeHash(stream);
    }
    
    [Benchmark]
    public void StandardHashXXHash64_ProtoBuff()
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, _testObject);
        var bytes = stream.ToArray();
        
        var _ = xxHash64.ComputeHash(bytes);
    }
    
    [Benchmark]
    public void StandardHashXXHash64_ProtoBuffStream()
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, _testObject);
        
        var _ = xxHash64.ComputeHash(stream);
    }
    
    [Benchmark]
    public void K4osHashXXHash64_Json()
    {
        var _ = XXH64.DigestOf(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_testObject)));
    }
    
    [Benchmark]
    public void K4osHashXXHash64_Json_IgnoreNulls()
    {
        var _ = XXH64.DigestOf(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_testObject, _jsonOptions)));
    }
    
    [Benchmark]
    public void K4osHashXXHash64_ProtoBuff()
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, _testObject);
        
        var _ = XXH64.DigestOf(stream.ToArray());
    }
    
    [Benchmark]
    public void CustomHash()
    {
        var _ = _customHashCalculator(_testObject);
    }
    
    [Benchmark]
    public void XK4osHashXXHash64_CustomSerialisation()
    {
        var state = new XXH64.State();
        var _ = _customXXHash64SerialisationFunction(_testObject, state);
    }
}