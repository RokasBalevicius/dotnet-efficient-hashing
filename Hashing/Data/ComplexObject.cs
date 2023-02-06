using ProtoBuf;

namespace Hashing.Data;

[ProtoContract]
public sealed class ComplexObject
{
    [ProtoMember(1)]
    public int Int1 { get; set; }
    
    [ProtoMember(2)]
    public int Int2 { get; set; }
    
    [ProtoMember(3)]
    public int Int3 { get; set; }
    
    [ProtoMember(4)]
    public int Int4 { get; set; }
    
    [ProtoMember(5)]
    public int? IntNullable1 { get; set; }
    
    [ProtoMember(6)]
    public int? IntNullable2 { get; set; }
    
    [ProtoMember(7)]
    public int? IntNullable3 { get; set; }
    
    [ProtoMember(8)]
    public int? IntNullable4 { get; set; }
    
    [ProtoMember(9)]
    public string String1 { get; set; } = "";
    
    [ProtoMember(10)]
    public string String2 { get; set; } = "";
    
    [ProtoMember(11)]
    public string String3 { get; set; } = "";
    
    [ProtoMember(12)]
    public string String4 { get; set; } = "";
    
    [ProtoMember(13)]
    public bool Bool1 { get; set; }
    
    [ProtoMember(14)]
    public bool Bool2 { get; set; }
    
    [ProtoMember(15)]
    public bool Bool3 { get; set; }
    
    [ProtoMember(16)]
    public bool Bool4 { get; set; }
    
    [ProtoMember(17)]
    public bool? BoolNullable1 { get; set; }
    
    [ProtoMember(18)]
    public bool? BoolNullable2 { get; set; }
    
    [ProtoMember(19)]
    public bool? BoolNullable3 { get; set; }
    
    [ProtoMember(20)]
    public bool? BoolNullable4 { get; set; }

    [ProtoMember(21)]
    public List<ComplexObject> ComplexObjects { get; set; } = new(0);
}    
