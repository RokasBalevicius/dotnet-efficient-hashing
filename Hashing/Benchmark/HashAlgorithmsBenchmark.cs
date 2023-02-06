using System.Data.HashFunction.CityHash;
using System.Data.HashFunction.MurmurHash;
using System.Data.HashFunction.xxHash;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Hashing.Data;
using InvertedTomato.IO;
using K4os.Hash.xxHash;
using Murmur;
using Standart.Hash.xxHash;

namespace Hashing.Benchmark;

[MemoryDiagnoser]
public class HashAlgorithmsBenchmark
{
    private byte[] _jsonTestObjectBytes;

    private readonly IxxHash _systemDataXXHash;
    private readonly IMurmurHash3 _systemDataMurmur3;
    private readonly ICityHash _systemDataCityHash;

    private readonly Murmur128 _murmurHashNetMurmur128NotManaged;
    private readonly Murmur128 _murmurHashNetMurmur128Managed;
    
    private readonly Crc _invertedTomatoCrc64;
    private readonly Crc _invertedTomatoCrc64We;
    private readonly Crc _invertedTomatoCrc64Xz;
    
    public HashAlgorithmsBenchmark()
    {
        var json = JsonSerializer.Serialize(TestDataGenerator.GenerateTestSampleWithLowFillLevel());
        //var json = JsonSerializer.Serialize(TestDataGenerator.GenerateTestSampleWithMediumFillLevel());
        //var json = JsonSerializer.Serialize(TestDataGenerator.GenerateTestSampleWithHighFillLevel());
        
        _jsonTestObjectBytes = Encoding.UTF8.GetBytes(json);

        _systemDataXXHash = xxHashFactory.Instance.Create(new xxHashConfig {HashSizeInBits = 64});
        _systemDataCityHash = CityHashFactory.Instance.Create(new CityHashConfig {HashSizeInBits = 64});
        _systemDataMurmur3 = MurmurHash3Factory.Instance.Create(new MurmurHash3Config {HashSizeInBits = 128});
        
        _murmurHashNetMurmur128NotManaged = MurmurHash.Create128(managed: false);
        _murmurHashNetMurmur128Managed = MurmurHash.Create128(managed: true);

        _invertedTomatoCrc64 = CrcAlgorithm.CreateCrc64();
        _invertedTomatoCrc64We = CrcAlgorithm.CreateCrc64We();
        _invertedTomatoCrc64Xz = CrcAlgorithm.CreateCrc64Xz();
    }

    [Benchmark]
    public void StandardHash_XXHash64()
    {
        var _ = xxHash64.ComputeHash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void SystemIoHash_XXHash64()
    {   
        var _ = XxHash64.Hash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void SystemData_XXHash64()
    {
        var _ = _systemDataXXHash.ComputeHash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void K4osHash_XXHash64()
    {
        var _ = XXH64.DigestOf(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void SystemData_Murmur3_128()
    {
        var _ = _systemDataMurmur3.ComputeHash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void MurmurHashNetUnmanagedHash_Murmur3_128()
    {
        var _ = _murmurHashNetMurmur128NotManaged.ComputeHash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void MurmurHashNetManagedHash_Murmur3_128()
    {
        var _ = _murmurHashNetMurmur128Managed.ComputeHash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void SystemData_CityHash()
    {
        var _ = _systemDataCityHash.ComputeHash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void CityHashNet_CityHash()
    {
        var _ = CityHash.CityHash.CityHash64(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void SystemIoHash_Crc64()
    {
        var _ = Crc64.Hash(_jsonTestObjectBytes);
    }
    
    [Benchmark]
    public void InvertedTomato_Crc64()
    {
        _invertedTomatoCrc64.Append(_jsonTestObjectBytes);
        var _ =_invertedTomatoCrc64.ToUInt64();
        _invertedTomatoCrc64.Clear();
    }
    
    [Benchmark]
    public void InvertedTomato_Crc64We()
    {
        _invertedTomatoCrc64We.Append(_jsonTestObjectBytes);
        var _ =_invertedTomatoCrc64We.ToUInt64();
        _invertedTomatoCrc64We.Clear();
    }
    
    [Benchmark]
    public void InvertedTomato_Crc64Xz()
    {
        _invertedTomatoCrc64Xz.Append(_jsonTestObjectBytes);
        var _ =_invertedTomatoCrc64Xz.ToUInt64();
        _invertedTomatoCrc64Xz.Clear();
    }
}