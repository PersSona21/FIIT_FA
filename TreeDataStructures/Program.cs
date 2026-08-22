using TreeDataStructures.Implementations.BST;

var tree = new BinarySearchTree<int, string>();

// Вставка
tree.Add(5, "five");
tree.Add(3, "three");
tree.Add(8, "eight");
tree.Add(1, "one");
tree.Add(4, "four");
tree.Add(7, "seven");
tree.Add(9, "nine");


Console.WriteLine($"Count после вставки: {tree.Count}");
Console.WriteLine();

Console.WriteLine("Order:");

foreach (var entry in tree.PostOrderReverse())
{
    Console.WriteLine(
        $"Key: {entry.Key}, Value: {entry.Value}, Depth: {entry.Depth}");
}