using Arithmetic.BigInt.Interfaces;


namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    private const int KaratsubaThreshold = 8;
    
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        if (BetterBigInteger.IsZero(a.GetDigits()) || BetterBigInteger.IsZero(b.GetDigits())) 
            return new BetterBigInteger([0]);
        var resultDigits = KaratsubaRecursive(a.GetDigits(), b.GetDigits());
        return new BetterBigInteger(resultDigits, a.IsNegative != b.IsNegative);

    }

    private static uint[] KaratsubaRecursive(ReadOnlySpan<uint> x, ReadOnlySpan<uint> y)
    {
        if (x.Length <= KaratsubaThreshold || y.Length <= KaratsubaThreshold)
        {
            return SimpleMultiplier.MultiplyCore(x, y);
        }

        var m = Math.Max(x.Length, y.Length) / 2;
        
        /*
         * ОБЫЧНО
         * (A*b + C) * (B*b + D) = AB*(b^2) + (AD + BC)*b + CD
         * 
         * КАРАЦУБА
         * (A + B) * (C + D) = AC + AD + BC + BD = (AD + BC) - AC - BD
         * так мы Экономим 1 операцию умножения, меняя её на 2 сложения
         */
        var xLow = x.Length > m ? x[..m] : x;
        var xHigh = x.Length > m ? x[m..] : [];
        
        var yLow = y.Length > m ? y[..m] : y;
        var yHigh = y.Length > m ? y[m..] : [];
        
        // ans = z2 * b^2 + z1 * b + z0
        var z0 = KaratsubaRecursive(xLow, yLow);
        var z2 = KaratsubaRecursive(xHigh, yHigh);
        
        var sumX = BetterBigInteger.AddValues(xLow, xHigh);
        var sumY = BetterBigInteger.AddValues(yLow, yHigh);
        var z1Full = KaratsubaRecursive(sumX, sumY);
        var z1 = BetterBigInteger.SubtractValues(z1Full, z0);
        z1 = BetterBigInteger.SubtractValues(z1, z2);

        var z1Shifted = ShiftDigits(z1, m);
        var z2Shifted = ShiftDigits(z2, 2 * m);
        
        var result = BetterBigInteger.AddValues(z0, z1Shifted);
        result = BetterBigInteger.AddValues(result, z2Shifted);
        return result;
    }

    private static uint[] ShiftDigits(uint[] digits, int shift)
    {
        if (BetterBigInteger.IsZero(digits)) return [0];
        
        var result = new uint[digits.Length + shift];
        digits.CopyTo(result, shift);
        return result;
    }
}