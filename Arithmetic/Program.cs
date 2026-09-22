using System.Diagnostics;
using Arithmetic.BigInt;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic;

public static class Program
{
    public static void Main()
    {
        DemoTrace();
        Console.WriteLine();
        BenchmarkMultipliers();
    }

    /// <summary>
    /// Небольшой пример для РУЧНОЙ проверки на бумаге.
    /// x = [1,2,3,4], y = [5,6,7,8]  (little-endian, 4 разряда каждое)
    /// Порог KaratsubaThreshold = 2, поэтому уже на первом разбиении (m=2)
    /// все под-вызовы (xLow*yLow, xHigh*yHigh, sumX*sumY) попадают в base case
    /// и считаются через SimpleMultiplier — удобно проверять вручную.
    ///
    /// Ожидаемая трассировка (см. пояснение в чате):
    ///   m = 2
    ///   xLow=[1,2] xHigh=[3,4]   yLow=[5,6] yHigh=[7,8]
    ///   z0 = xLow*yLow   = [5, 16, 12, 0]
    ///   z2 = xHigh*yHigh = [21, 52, 32, 0]
    ///   sumX=[4,6]  sumY=[12,14]
    ///   z1_full = sumX*sumY = [48, 128, 84, 0]
    ///   z1 = z1_full - z0 - z2 = [22, 60, 40]
    ///   result = z0 + (z1 << 2 разряда) + (z2 << 4 разряда)
    ///          = [5, 16, 34, 60, 61, 52, 32, 0]
    /// </summary>
    private static void DemoTrace()
    {
        Console.WriteLine("=== Демонстрация: ручная трассировка Карацубы ===");

        var x = new BetterBigInteger([1, 2, 3, 4]);
        var y = new BetterBigInteger([5, 6, 7, 8]);

        Console.WriteLine($"x = {x} (digits: [{string.Join(", ", x.GetDigits().ToArray())}])");
        Console.WriteLine($"y = {y} (digits: [{string.Join(", ", y.GetDigits().ToArray())}])");

        var simpleResult = new SimpleMultiplier().Multiply(x, y);
        var karatsubaResult = new KaratsubaMultiplier().Multiply(x, y);

        Console.WriteLine($"SimpleMultiplier:    {simpleResult}");
        Console.WriteLine($"                     digits: [{string.Join(", ", simpleResult.GetDigits().ToArray())}]");
        Console.WriteLine($"KaratsubaMultiplier: {karatsubaResult}");
        Console.WriteLine($"                     digits: [{string.Join(", ", karatsubaResult.GetDigits().ToArray())}]");
        Console.WriteLine($"Совпадают: {simpleResult == karatsubaResult}");

        Console.WriteLine();
        Console.WriteLine("Ожидаемые (посчитанные вручную) digits результата:");
        Console.WriteLine("[5, 16, 34, 60, 61, 52, 32, 0]");
    }

    /// <summary>
    /// Сравнение производительности SimpleMultiplier и KaratsubaMultiplier
    /// на числах разного размера. Помогает подобрать порог переключения
    /// стратегии в operator *.
    /// </summary>
    private static void BenchmarkMultipliers()
    {
        Console.WriteLine("=== Сравнение производительности ===");
        Console.WriteLine($"{"Разрядов",10} | {"Simple, мс",12} | {"Karatsuba, мс",14} | Быстрее");
        Console.WriteLine(new string('-', 55));

        // Размеры в РАЗРЯДАХ (uint), не в битах/десятичных цифрах.
        int[] sizes = [4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048];
        var rnd = new Random(52);

        var simple = new SimpleMultiplier();
        var karatsuba = new KaratsubaMultiplier();

        foreach (var size in sizes)
        {
            var aDigits = RandomDigits(rnd, size);
            var bDigits = RandomDigits(rnd, size);
            var a = new BetterBigInteger(aDigits);
            var b = new BetterBigInteger(bDigits);

            // Прогрев (JIT), не измеряем
            simple.Multiply(a, b);
            karatsuba.Multiply(a, b);

            const int iterations = 5;

            var swSimple = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) simple.Multiply(a, b);
            swSimple.Stop();

            var swKaratsuba = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) karatsuba.Multiply(a, b);
            swKaratsuba.Stop();

            double simpleMs = swSimple.Elapsed.TotalMilliseconds / iterations;
            double karatsubaMs = swKaratsuba.Elapsed.TotalMilliseconds / iterations;
            string faster = simpleMs < karatsubaMs ? "Simple" : "Karatsuba";

            Console.WriteLine($"{size,10} | {simpleMs,12:F4} | {karatsubaMs,14:F4} | {faster}");
        }
    }

    private static uint[] RandomDigits(Random rnd, int length)
    {
        var digits = new uint[length];
        for (int i = 0; i < length; i++)
        {
            var buffer = new byte[4];
            rnd.NextBytes(buffer);
            digits[i] = BitConverter.ToUInt32(buffer, 0);
        }
        // старший разряд не должен быть 0, чтобы число было ровно нужной длины
        if (digits[^1] == 0) digits[^1] = 1;
        return digits;
    }
}