namespace Hashing.Data;

public static class TestDataGenerator
{
    private static Random _random = new ();
    
    public static TestObject GenerateTestSampleWithLowFillLevel()
    {
        return new TestObject();
    }
    
    public static TestObject GenerateTestSampleWithMediumFillLevel()
    {
        return new TestObject
        {
            BoolNullable1 = true,
            BoolNullable2 = true,
            BoolNullable3 = true,
            BoolNullable4 = true,
            String1 = GenerateRandomString(64),
            String2 = GenerateRandomString(64),
            String3 = GenerateRandomString(64),
            String4 = GenerateRandomString(64),
            StringNullable1 = GenerateRandomString(64),
            StringNullable2 = GenerateRandomString(64),
            StringNullable3 = GenerateRandomString(64),
            StringNullable4 = GenerateRandomString(64),
            IntNullable1 = 123445678,
            IntNullable2 = 123445678,
            IntNullable3 = 123445678,
            IntNullable4 = 123445678,
            ListOfBool1 = GenerateListOfBool(64),
            ListOfBool2 = GenerateListOfBool(64),
            ListOfBool3 = GenerateListOfBool(64),
            ListOfBool4 = GenerateListOfBool(64),
            ListOfInt1 = GenerateListOfInt(64),
            ListOfInt2 = GenerateListOfInt(64),
            ListOfInt3 = GenerateListOfInt(64),
            ListOfInt4 = GenerateListOfInt(64),
            ListOfString1 = GenerateListOfString(64, 64),
            ListOfString2 = GenerateListOfString(64, 64),
            ListOfString3 = GenerateListOfString(64, 64),
            ListOfString4 = GenerateListOfString(64, 64),
            DicOfString1 = GenerateDicOfString(64, 64),
            DicOfString2 = GenerateDicOfString(64, 64),
            ListOfComplex1 = GenerateListOfComplex(8, false),
            ListOfComplex2 = GenerateListOfComplex(8, false),
            DicOfComplex1 = GenerateDicOfComplex(8, 64),
            DicOfComplex2 = GenerateDicOfComplex(8, 64),
        };
    }

    public static TestObject GenerateTestSampleWithHighFillLevel()
    {
        var result = GenerateTestSampleWithMediumFillLevel();
        result.BoolNullable5 = true;
        result.BoolNullable6 = true;
        result.BoolNullable7 = true;
        result.BoolNullable8 = true;
        result.String5 = GenerateRandomString(64);
        result.String6 = GenerateRandomString(64);
        result.String7 = GenerateRandomString(64);
        result.String8 = GenerateRandomString(64);
        result.StringNullable5 = GenerateRandomString(64);
        result.StringNullable6 = GenerateRandomString(64);
        result.StringNullable7 = GenerateRandomString(64);
        result.StringNullable8 = GenerateRandomString(64);
        result.IntNullable5 = 123445678;
        result.IntNullable6 = 123445678;
        result.IntNullable7 = 123445678;
        result.IntNullable8 = 123445678;
        result.ListOfBool5 = GenerateListOfBool(64);
        result.ListOfBool6 = GenerateListOfBool(64);
        result.ListOfBool7 = GenerateListOfBool(64);
        result.ListOfBool8 = GenerateListOfBool(64);
        result.ListOfInt5 = GenerateListOfInt(64);
        result.ListOfInt6 = GenerateListOfInt(64);
        result.ListOfInt7 = GenerateListOfInt(64);
        result.ListOfInt8 = GenerateListOfInt(64);
        result.ListOfString5 = GenerateListOfString(64, 64);
        result.ListOfString6 = GenerateListOfString(64, 64);
        result.ListOfString7 = GenerateListOfString(64, 64);
        result.ListOfString8 = GenerateListOfString(64, 64);
        result.DicOfString3 = GenerateDicOfString(64, 64);
        result.DicOfString4 = GenerateDicOfString(64, 64);
        result.ListOfComplex3 = GenerateListOfComplex(8, true);
        result.ListOfComplex4 = GenerateListOfComplex(8, true);
        result.DicOfComplex3 = GenerateDicOfComplex(8, 64);
        result.DicOfComplex4 = GenerateDicOfComplex(8, 64);

        return result;
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
    
    private static List<string> GenerateListOfString(int count, int stringLength)
    {
        var result = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            result.Add(GenerateRandomString(stringLength));
        }

        return result;
    }
    
    private static Dictionary<string, ComplexObject> GenerateDicOfComplex(int count, int keyLength)
    {
        var result = new Dictionary<string, ComplexObject>(count);
        for (var i = 0; i < count; i++)
        {
            result.Add(GenerateRandomString(keyLength), GenerateComplexObject(false));
        }

        return result;
    }
    
    private static Dictionary<string, string> GenerateDicOfString(int count, int stringLength)
    {
        var result = new Dictionary<string, string>(count);
        for (var i = 0; i < count; i++)
        {
            result.Add(GenerateRandomString(stringLength), GenerateRandomString(stringLength));
        }

        return result;
    }
    
    private static List<bool> GenerateListOfBool(int length)
    {
        var result = new List<bool>(length);
        for (var i = 0; i < length; i++)
        {
            result.Add(true);
        }

        return result;
    }
    
    private static List<int> GenerateListOfInt(int length)
    {
        var result = new List<int>(length);
        for (var i = 0; i < length; i++)
        {
            result.Add(12345678);
        }

        return result;
    }

    private static ComplexObject GenerateComplexObject(bool generateNested)
    {
        var result = new ComplexObject
        {
            BoolNullable1 = true,
            BoolNullable2 = false,
            BoolNullable3 = true,
            BoolNullable4 = false,
            String1 = GenerateRandomString(64),
            String2 = GenerateRandomString(64),
            String3 = GenerateRandomString(64),
            String4 = GenerateRandomString(64),
        };

        if (generateNested)
        {
            result.ComplexObjects = GenerateListOfComplex(32, false);
        }

        return result;
    }
    
    private static List<ComplexObject> GenerateListOfComplex(int length, bool generateNested)
    {
        var result = new List<ComplexObject>(length);
        for (var i = 0; i < length; i++)
        {
            result.Add(GenerateComplexObject(generateNested));
        }

        return result;
    }
}