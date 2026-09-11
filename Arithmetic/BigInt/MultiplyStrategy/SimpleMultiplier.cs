using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        var result = new uint[a.GetDigits().Length + b.GetDigits().Length];
        var digits1 = a.GetDigits();
        var digits2 = b.GetDigits();
        for (var i = 0; i < digits1.Length; ++i)
        {
            uint carry = 0;
            for (var j = 0; j < digits2.Length; ++j)
            {
                ulong product = (ulong)digits1[i] * digits2[j] + result[i + j] + carry;
                result[i + j] = (uint)product;
                carry = (uint)(product >> 32);
            }

            result[i + digits2.Length] += carry;
        }

        if (a.IsNegative == b.IsNegative) return new BetterBigInteger(result, false);
        else return new BetterBigInteger(result, true);
    }

}