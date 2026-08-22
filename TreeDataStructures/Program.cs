using TreeDataStructures.Implementations.BST;

var tree = new BinarySearchTree<int, string>();

// Вставка
tree.Add(5, "five");
tree.Add(3, "three");
tree.Add(8, "eight");
tree.Add(1, "one");
tree.Add(4, "four");

Console.WriteLine($"Count после вставки: {tree.Count}"); // ожидаем 5