using ProtoBuf;

namespace Hashing.Data;

[ProtoContract]
public sealed class TestObject
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
    public int Int5 { get; set; }
    
    [ProtoMember(6)]
    public int Int6 { get; set; }
    
    [ProtoMember(7)]
    public int Int7 { get; set; }
    
    [ProtoMember(8)]
    public int Int8 { get; set; }
    
    [ProtoMember(9)]
    public int? IntNullable1 { get; set; }
    
    [ProtoMember(10)]
    public int? IntNullable2 { get; set; }
    
    [ProtoMember(11)]
    public int? IntNullable3 { get; set; }
    
    [ProtoMember(12)]
    public int? IntNullable4 { get; set; }
    
    [ProtoMember(13)]
    public int? IntNullable5 { get; set; }
    
    [ProtoMember(14)]
    public int? IntNullable6 { get; set; }
    
    [ProtoMember(15)]
    public int? IntNullable7 { get; set; }
    
    [ProtoMember(16)]
    public int? IntNullable8 { get; set; }

    [ProtoMember(17)]
    public string String1 { get; set; } = "";
    
    [ProtoMember(18)]
    public string String2 { get; set; } = "";
    
    [ProtoMember(19)]
    public string String3 { get; set; } = "";
    
    [ProtoMember(20)]
    public string String4 { get; set; } = "";
    
    [ProtoMember(21)]
    public string String5 { get; set; } = "";
    
    [ProtoMember(22)]
    public string String6 { get; set; } = "";
    
    [ProtoMember(23)]
    public string String7 { get; set; } = "";
    
    [ProtoMember(24)]
    public string String8 { get; set; } = "";
    
    [ProtoMember(25)]
    public string? StringNullable1 { get; set; }
    
    [ProtoMember(26)]
    public string? StringNullable2 { get; set; }
    
    [ProtoMember(27)]
    public string? StringNullable3 { get; set; }
    
    [ProtoMember(28)]
    public string? StringNullable4 { get; set; }
    
    [ProtoMember(29)]
    public string? StringNullable5 { get; set; }
    
    [ProtoMember(30)]
    public string? StringNullable6 { get; set; }
    
    [ProtoMember(31)]
    public string? StringNullable7 { get; set; }
    
    [ProtoMember(32)]
    public string? StringNullable8 { get; set; }
    
    [ProtoMember(33)]
    public bool Bool1 { get; set; }
    
    [ProtoMember(34)]
    public bool Bool2 { get; set; }
    
    [ProtoMember(35)]
    public bool Bool3 { get; set; }
    
    [ProtoMember(36)]
    public bool Bool4 { get; set; }
    
    [ProtoMember(37)]
    public bool Bool5 { get; set; }
    
    [ProtoMember(38)]
    public bool Bool6 { get; set; }
    
    [ProtoMember(39)]
    public bool Bool7 { get; set; }
    
    [ProtoMember(40)]
    public bool Bool8 { get; set; }
    
    [ProtoMember(41)]
    public bool? BoolNullable1 { get; set; }
    
    [ProtoMember(42)]
    public bool? BoolNullable2 { get; set; }
    
    [ProtoMember(43)]
    public bool? BoolNullable3 { get; set; }
    
    [ProtoMember(44)]
    public bool? BoolNullable4 { get; set; }
    
    [ProtoMember(45)]
    public bool? BoolNullable5 { get; set; }
    
    [ProtoMember(46)]
    public bool? BoolNullable6 { get; set; }
    
    [ProtoMember(47)]
    public bool? BoolNullable7 { get; set; }
    
    [ProtoMember(48)]
    public bool? BoolNullable8 { get; set; }

    [ProtoMember(49)]
    public List<int> ListOfInt1 { get; set; } = new(0);
    
    [ProtoMember(50)]
    public List<int> ListOfInt2 { get; set; } = new(0);
    
    [ProtoMember(51)]
    public List<int> ListOfInt3 { get; set; } = new(0);
    
    [ProtoMember(52)]
    public List<int> ListOfInt4 { get; set; } = new(0);
    
    [ProtoMember(53)]
    public List<int> ListOfInt5 { get; set; } = new(0);
    
    [ProtoMember(54)]
    public List<int> ListOfInt6 { get; set; } = new(0);
    
    [ProtoMember(55)]
    public List<int> ListOfInt7 { get; set; } = new(0);
    
    [ProtoMember(56)]
    public List<int> ListOfInt8 { get; set; } = new(0);

    [ProtoMember(57)]
    public List<string> ListOfString1 { get; set; } = new(0);
    
    [ProtoMember(58)]
    public List<string> ListOfString2 { get; set; } = new(0);
    
    [ProtoMember(59)]
    public List<string> ListOfString3 { get; set; } = new(0);
    
    [ProtoMember(60)]
    public List<string> ListOfString4 { get; set; } = new(0);
    
    [ProtoMember(61)]
    public List<string> ListOfString5 { get; set; } = new(0);
    
    [ProtoMember(62)]
    public List<string> ListOfString6 { get; set; } = new(0);
    
    [ProtoMember(63)]
    public List<string> ListOfString7 { get; set; } = new(0);
    
    [ProtoMember(64)]
    public List<string> ListOfString8 { get; set; } = new(0);
    
    [ProtoMember(65)]
    public List<bool> ListOfBool1 { get; set; } = new(0);
    
    [ProtoMember(66)]
    public List<bool> ListOfBool2 { get; set; } = new(0);
    
    [ProtoMember(67)]
    public List<bool> ListOfBool3 { get; set; } = new(0);
    
    [ProtoMember(68)]
    public List<bool> ListOfBool4 { get; set; } = new(0);
    
    [ProtoMember(69)]
    public List<bool> ListOfBool5 { get; set; } = new(0);
    
    [ProtoMember(70)]
    public List<bool> ListOfBool6 { get; set; } = new(0);
    
    [ProtoMember(71)]
    public List<bool> ListOfBool7 { get; set; } = new(0);
    
    [ProtoMember(72)]
    public List<bool> ListOfBool8 { get; set; } = new(0);

    [ProtoMember(73)]
    public List<ComplexObject> ListOfComplex1 { get; set; } = new(0);
    
    [ProtoMember(74)]
    public List<ComplexObject> ListOfComplex2 { get; set; } = new(0);
    
    [ProtoMember(75)]
    public List<ComplexObject> ListOfComplex3 { get; set; } = new(0);
    
    [ProtoMember(76)]
    public List<ComplexObject> ListOfComplex4 { get; set; } = new(0);

    [ProtoMember(77)]
    public Dictionary<string, string> DicOfString1 { get; set; } = new(0);
    
    [ProtoMember(78)]
    public Dictionary<string, string> DicOfString2 { get; set; } = new(0);
    
    [ProtoMember(79)]
    public Dictionary<string, string> DicOfString3 { get; set; } = new(0);
    
    [ProtoMember(80)]
    public Dictionary<string, string> DicOfString4 { get; set; } = new(0);
    
    [ProtoMember(81)]
    public Dictionary<string, ComplexObject> DicOfComplex1 { get; set; } = new(0);
    
    [ProtoMember(82)]
    public Dictionary<string, ComplexObject> DicOfComplex2 { get; set; } = new(0);
    
    [ProtoMember(83)]
    public Dictionary<string, ComplexObject> DicOfComplex3 { get; set; } = new(0);
    
    [ProtoMember(84)]
    public Dictionary<string, ComplexObject> DicOfComplex4 { get; set; } = new(0);
}