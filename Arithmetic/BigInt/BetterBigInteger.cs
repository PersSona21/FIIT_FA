using System.Runtime.CompilerServices;
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
        : this(digits.ToArray(), isNegative)
    {
    }
    
    public BetterBigInteger(string value, int radix)
    {
        throw new NotImplementedException();
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
    
    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    
    
    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
       => throw new NotImplementedException("Умножение делегируется стратегии, выбирать необходимо в зависимости от размеров чисел");

    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        return -a - new BetterBigInteger([1], false);
    }
    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator <<(BetterBigInteger a, int shift) => throw new NotImplementedException();
    public static BetterBigInteger operator >> (BetterBigInteger a, int shift) => throw new NotImplementedException();
    
    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;
    
    public override string ToString() => ToString(10);
    public string ToString(int radix) => throw new NotImplementedException();
    
}