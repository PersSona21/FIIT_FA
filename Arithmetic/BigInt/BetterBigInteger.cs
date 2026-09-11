using System.Runtime.CompilerServices;
using System.Text;
using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;
    
    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;
    
    public bool IsNegative => _signBit == 1;

    private static int GetSignificantLength(uint[] digits)
    {
        int len = digits.Length;
        while (len > 1 && digits[len - 1] == 0)
            len--;
        return len;
    }
    
    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        if (digits.Length == 0) throw new ArgumentException("Массив не должен быть пустым");
        
        int len = GetSignificantLength(digits);
        
        if (len == 1)
        {
            _smallValue = digits[0];
            _data = null;
        }
        else
        {
            _data = new uint[len];
            Array.Copy(digits, _data, len);
        }

        _signBit = isNegative ? 1 : 0;
        if (_data == null && _smallValue == 0) _signBit = 0;
    }
    
    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false)
        : this(digits.ToArray(), isNegative) {}
    
    public BetterBigInteger(string value, int radix)
    {
        if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException("radix", "Основание должно быть от 2 до 36");
        if (value.Length == 0 || (value.Length == 1 && (value[0] == '-' || value[0] == '+'))) throw new ArgumentException("Строка не корректна");
        bool flag = false;
        if (value[0] == '-' || value[0] == '+')
        {
            if (value[0] == '-') flag = true;
            value = value[1..]; // убираю знак
        }

        BetterBigInteger result = new BetterBigInteger([0]);
        BetterBigInteger radixBig = new BetterBigInteger([(uint)radix]);

        uint num;
        foreach (var ch in value.ToLower())
        {
            if (ch >= '0' && ch <= '9') num = (uint)(ch - '0');
            else if (ch >= 'a' && ch <= 'z') num = (uint)(ch - 'a' + 10);
            else throw new ArgumentException("Число не корректное");
            if (num >= radix) throw new ArgumentException("Число не корректное");
            BetterBigInteger digitBig = new BetterBigInteger([num]);
            result = result * radixBig + digitBig;
        }

        _data = result._data;
        _smallValue = result._smallValue;
        _signBit = flag ? 1 : 0;
        if (_data == null && _smallValue == 0) _signBit = 0;
    }
    
    public ReadOnlySpan<uint> GetDigits()
    {
        return _data ?? [_smallValue];
    }

    private static int CompareMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        var sign = 1;
        
        if (a.Length > b.Length) return sign;
        if (a.Length < b.Length) return -sign;
        
        for (int i = a.Length - 1; i >= 0; --i)
        {
            if (a[i] > b[i]) return sign;
            if (a[i] < b[i]) return -sign;
        }

        return 0;
    }
    
    public int CompareTo(IBigInteger? other)
    {
        if (other == null) throw new ArgumentException("Другой биг инт не должен быть null");
        int sign = this.IsNegative ? -1 : 1;
        if (this.IsNegative != other.IsNegative)
        {
            if (other.IsNegative) return 1;
            return -1;
        }

        int magnitudeCompare = CompareMagnitudes(this.GetDigits(), other.GetDigits());
        
        return this.IsNegative ? -magnitudeCompare : magnitudeCompare;
    }

    public bool Equals(IBigInteger? other) => other != null && this.CompareTo(other) == 0;
    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);

    // через встроенный System.HashCode
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var num in this.GetDigits())
        {
            hash.Add(num);
        }
        hash.Add(this.IsNegative);
        return hash.ToHashCode();
    }

    private static uint[] AddValues(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        var maxLen = Math.Max(a.Length, b.Length);
        uint carry = 0;
        uint[] result = new uint[maxLen + 1];

        for (int i = 0; i < maxLen; ++i)
        {
            uint digitA = (a.Length > i) ? a[i] : 0;
            uint digitB = (b.Length > i) ? b[i] : 0;
            ulong sum = (ulong)digitA + digitB + carry;
            
            result[i] = (uint)sum;
            carry = (uint)(sum >> 32);
        }

        if (carry == 0) Array.Resize(ref result, maxLen);
        return result;
    }

    private static uint[] SubtractValues(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        var maxLen = a.Length;
        uint[] result = new uint[maxLen];
        uint carry = 0;

        for (int i = 0; i < maxLen; ++i)
        {
            uint digitA = a[i];
            uint digitB = (b.Length > i) ? b[i] : 0;
            ulong sum;
            if (digitA >= ((ulong)digitB + carry))
            {
                sum = digitA - digitB - carry;
                carry = 0;
            }
            else
            {
                sum = (1ul << 32) + digitA - digitB - carry;
                carry = 1;
            }

            result[i] = (uint)sum;
        }
    
        int len = GetSignificantLength(result);
        if (len != result.Length) Array.Resize(ref result, len);
    
        return result;
    }
    
    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b)
    {
        uint[] digits;
        bool flag;
        if (a.IsNegative == b.IsNegative)
        {
            digits = AddValues(a.GetDigits(), b.GetDigits());
            flag = a.IsNegative;
        }
        else
        {
            int magnitudeCompare = CompareMagnitudes(a.GetDigits(), b.GetDigits());
            if (magnitudeCompare == 0)
            {
                digits = [0];
                flag = false;
            }
            else if (magnitudeCompare > 0)
            {
                digits = SubtractValues(a.GetDigits(), b.GetDigits());
                flag = a.IsNegative;
            }
            else
            {
                digits = SubtractValues(b.GetDigits(), a.GetDigits());
                flag = b.IsNegative;
            }
        }

        return new BetterBigInteger(digits, flag);
    }

    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b) => a + (-b);

    public static BetterBigInteger operator -(BetterBigInteger a)
    {
        return new BetterBigInteger(a.GetDigits().ToArray(), (!a.IsNegative));
    }

    private static (uint[], uint) SimpleDivideNumber(ReadOnlySpan<uint> a, uint b)
    {
        var result = new uint[a.Length];

        uint reminder = 0;

        for (var i = a.Length - 1; i >= 0; --i)
        {
            ulong current = reminder * (1ul << 32) + a[i];
            result[i] = (uint)(current / b);
            reminder = (uint)(current % b);
        }
        
        return (result, reminder);
    }
    
    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();


    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    {
        var mult = new SimpleMultiplier();
        return mult.Multiply(a, b);
    }
    
    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        return -a - new BetterBigInteger([1], false);
    }

    // в доп код
    private static uint[] ToTwosComplement(ReadOnlySpan<uint> digits, bool isNegative, int length)
    {
        var result = new uint[length];
        for (var i = 0; i < length; ++i)
        {
            if (isNegative) result[i] = (i < digits.Length) ? ~digits[i] : ~0u;
            else result[i] = (i < digits.Length) ? digits[i] : 0;
        }

        if (isNegative) result = AddValues(result, [1]);
        
        return result;
    }

    // из доп кода
    private static BetterBigInteger FromTwosComplement(uint[] digits)
    {
        var length = digits.Length;
        if (digits[length-1] >> 31 == 1)
        {
            var subtracted = SubtractValues(digits, [1]);
            var result = new uint[length];
            for (var i = 0; i < length; ++i)
            {
                result[i] = (i < subtracted.Length) ? ~subtracted[i] : ~0u;
            }

            return new BetterBigInteger(result, true);
        }
        else return new BetterBigInteger(digits, false);
    }

    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b)
    {
        var maxLen = Math.Max(a.GetDigits().Length, b.GetDigits().Length) + 1;
        var result = new uint[maxLen];
        var a1 = ToTwosComplement(a.GetDigits(), a.IsNegative, maxLen);
        var b1 = ToTwosComplement(b.GetDigits(), b.IsNegative, maxLen);
        for (int i = 0; i < maxLen; ++i)
        {
            result[i] = a1[i] & b1[i];
        }
        
        return FromTwosComplement(result);
    }
    
    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b){
        var maxLen = Math.Max(a.GetDigits().Length, b.GetDigits().Length) + 1;
        var result = new uint[maxLen];
        var a1 = ToTwosComplement(a.GetDigits(), a.IsNegative, maxLen);
        var b1 = ToTwosComplement(b.GetDigits(), b.IsNegative, maxLen);
        for (int i = 0; i < maxLen; ++i)
        {
            result[i] = a1[i] | b1[i];
        }
        
        return FromTwosComplement(result);
    }
    
    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b){
        var maxLen = Math.Max(a.GetDigits().Length, b.GetDigits().Length) + 1;
        var result = new uint[maxLen];
        var a1 = ToTwosComplement(a.GetDigits(), a.IsNegative, maxLen);
        var b1 = ToTwosComplement(b.GetDigits(), b.IsNegative, maxLen);
        for (int i = 0; i < maxLen; ++i)
        {
            result[i] = a1[i] ^ b1[i];
        }
        
        return FromTwosComplement(result);
    }

    public static BetterBigInteger operator <<(BetterBigInteger a, int shift)
    {
        if (shift < 0) throw new ArgumentOutOfRangeException("shift");
        else if (shift == 0) return a;

        var digits = a.GetDigits();
        
        var wordShift = shift / 32;
        var bitShift = shift % 32;

        var result = new uint[digits.Length + wordShift + 1];
        
        for (int i = 0; i < digits.Length; ++i)
        {
            uint lowPart = digits[i] << bitShift;
            uint highPart = (bitShift == 0) ? 0 : digits[i] >> (32 - bitShift);
            
            result[i + wordShift] |= lowPart;
            result[i + wordShift + 1] |= highPart;
        }
        
        return new BetterBigInteger(result, a.IsNegative);
    }
    
    public static BetterBigInteger operator >> (BetterBigInteger a, int shift)
    {
        if (shift < 0) throw new ArgumentOutOfRangeException("shift");
        else if (shift == 0) return a;

        var digits = ToTwosComplement(a.GetDigits(), a.IsNegative, a.GetDigits().Length);
        if (shift >= digits.Length * 32) 
        {
            return a.IsNegative ? new BetterBigInteger([1], true) : new BetterBigInteger([0]);
        }
        
        var wordShift = shift / 32;
        var bitShift = shift % 32;

        var result = new uint[Math.Max(digits.Length - wordShift, 1)];
        
        for (int i = 0; i < result.Length; ++i)
        {
            var idx1 = i + wordShift;
            var idx2 = i + wordShift + 1;

            var signExtension = a.IsNegative ? 0xFFFFFFFF : 0;

            var digit1 = (idx1 < digits.Length) ? digits[idx1] : signExtension;
            var digit2 = (idx2 < digits.Length) ? digits[idx2] : signExtension;

            uint lowPart = digit1 >> bitShift;
            uint highPart = (bitShift == 0) ? 0 : digit2 << (32 - bitShift);

            result[i] = lowPart | highPart;
        }
        
        return FromTwosComplement(result);
    }
    
    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;
    
    public override string ToString() => ToString(10);

    private static bool IsZero(ReadOnlySpan<uint> digits)
    {
        foreach (var d in digits)
            if (d != 0) return false;
        return true;
    }
    
    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException("radix", "Основание должно быть от 2 до 36");
        if (IsZero(GetDigits())) return "0";
        var data = GetDigits().ToArray();
        var chars = new List<char>();
        while (!IsZero(data))
        {
            var (a,b) = SimpleDivideNumber(data, (uint)radix);
            char ch = b < 10 ? (char)('0' + b) : (char)('a' + (b - 10));
            chars.Add(ch);
            data = a;
        }
        if (this.IsNegative) chars.Add('-');
        chars.Reverse();
        return new string(chars.ToArray());
    }
    
}