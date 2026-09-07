using Arithmetic.BigInt;

using System.Numerics;

static void Print(BetterBigInteger a)
{
    foreach (var var in a.GetDigits())
    {
        Console.Write(var);
    }
    Console.WriteLine();
}

static void PrintBits(byte value)
{
    Console.WriteLine(
        Convert.ToString(value, 2).PadLeft(8, '0')
    );
}

uint[] data1 = [100, 200, 300, 400];
uint[] data2 = [99, 300, 200, 400];

BigInteger a = 7;
BigInteger b = -3;
BigInteger c = 3;
BetterBigInteger a1 = new BetterBigInteger([7]);
BetterBigInteger b1 = new BetterBigInteger([3], true);
BetterBigInteger c1 = new BetterBigInteger([3]);

// Console.WriteLine(a & c); // 3
// Console.WriteLine(a & b); // 5
// Console.WriteLine(a | c); // 7
// Console.WriteLine(a | b); // -1

// Print(a1 & c1);
// Print(a1 & b1);
// Print(a1 | b1);