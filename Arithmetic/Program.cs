using Arithmetic.BigInt;
// using Arithmetic.BigInt.MultiplyStrategy;
using System.Numerics;


static void Print(BetterBigInteger a)
{
    // var res = BetterBigInteger.ToTwosComplement(a.GetDigits(), a.IsNegative, a.GetDigits().Length);
    foreach (var var in a.GetDigits())
    {
        Console.Write($"{var} ");
    }
    Console.WriteLine();
}


uint[] data1 = [1, 3, 5];
uint[] data2 = [6, 3];

BetterBigInteger a = new BetterBigInteger(data1);
BetterBigInteger b = new BetterBigInteger(data2);
// var d = new SimpleMultiplier();

var c = a * b;

Print(c);