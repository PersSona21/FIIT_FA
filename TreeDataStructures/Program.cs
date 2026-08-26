using TreeDataStructures.Implementations.BST;

int a = 10, b = 12, c = 9, d = 10;

var cmp = Comparer<int>.Default.Compare(a, d);

Console.WriteLine(cmp <= 0);