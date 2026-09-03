using Arithmetic.BigInt;

uint[] data1 = [100, 200, 300, 400];
uint[] data2 = [99, 300, 200, 400];

BetterBigInteger a = new BetterBigInteger(data1, false);
BetterBigInteger b = new BetterBigInteger(data2, true);
BetterBigInteger c = a + b;


Console.WriteLine(c.IsNegative);