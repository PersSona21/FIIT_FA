using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    private const uint HalfMask = 0xFFFF;

    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        if (BetterBigInteger.IsZero(a.GetDigits()) || BetterBigInteger.IsZero(b.GetDigits())) 
            return new BetterBigInteger([0]);
        var result = MultiplyCore(a.GetDigits(), b.GetDigits());
        return new BetterBigInteger(result, a.IsNegative != b.IsNegative);
    }
    
    internal static uint[] MultiplyCore(ReadOnlySpan<uint> digits1, ReadOnlySpan<uint> digits2)
    {
        var result = new uint[digits1.Length + digits2.Length];

        for (var i = 0; i < digits1.Length; ++i)
        {
            uint carry = 0;

            for (var j = 0; j < digits2.Length; ++j)
            {
                // uint a = A * 2^16 + C
                // uint b = B * 2^16 + D

                uint A = digits1[i] >> 16;
                uint C = digits1[i] & HalfMask;

                uint B = digits2[j] >> 16;
                uint D = digits2[j] & HalfMask;

                // (A * base + C) * (B * base + D)
                //
                // = A*B*base^2
                // + (A*D + C*B)*base
                // + C*D

                uint AB = A * B;
                uint AD = A * D;
                uint CB = C * B;
                uint CD = C * D;

                // AD + CB
                uint middleLow = AD + CB;
                uint middleHigh = middleLow < AD ? 1u : 0u;

                // (AD + CB) << 16
                uint shiftedLow = middleLow << 16;
                uint shiftedHigh =
                    (middleHigh << 16) |
                    (middleLow >> 16);

                // CD + shiftedLow
                uint low = CD + shiftedLow;
                uint lowCarry = low < CD ? 1u : 0u;

                // AB + shiftedHigh + перенос из младшей части
                uint high = AB + shiftedHigh + lowCarry;

                // Добавляем полученное произведение к result[i + j]
                // и перенос от предыдущего умножения.
                uint sum = low + result[i + j];
                uint carry1 = sum < low ? 1u : 0u;

                sum += carry;
                uint carry2 = sum < carry ? 1u : 0u;

                result[i + j] = sum;

                carry = high + carry1 + carry2;
            }

            result[i + digits2.Length] = carry;
        }

        return result;
    }
}