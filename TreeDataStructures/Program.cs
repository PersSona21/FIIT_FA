using TreeDataStructures.Implementations.BST;

int passed = 0, failed = 0;

void Check(string name, bool condition)
{
    if (condition) { passed++; Console.WriteLine($"[OK]   {name}"); }
    else { failed++; Console.WriteLine($"[FAIL] {name}"); }
}

BinarySearchTree<int, string> BuildTree(params int[] keys)
{
    var t = new BinarySearchTree<int, string>();
    foreach (var k in keys) t.Add(k, $"v{k}");
    return t;
}

void CheckSorted(string name, BinarySearchTree<int, string> tree, params int[] expected)
{
    var actual = tree.InOrder().Select(e => e.Key).ToArray();
    Check(name, actual.SequenceEqual(expected));
    if (!actual.SequenceEqual(expected))
        Console.WriteLine($"       expected: [{string.Join(",", expected)}], actual: [{string.Join(",", actual)}]");
}

// ---------- Add / поиск ----------
var t1 = BuildTree(5, 3, 8, 1, 4);
Check("Add: Count == 5", t1.Count == 5);
Check("Add: ContainsKey(3)", t1.ContainsKey(3));
Check("Add: ContainsKey(100) == false", !t1.ContainsKey(100));
t1.Add(3, "UPDATED");
Check("Add: обновление значения не меняет Count", t1.Count == 5);
Check("Add: обновление значения читается", t1.TryGetValue(3, out var upd) && upd == "UPDATED");

// ---------- Обходы ----------
var t2 = BuildTree(5, 3, 8, 1, 4, 7, 9);
CheckSorted("InOrder", t2, 1, 3, 4, 5, 7, 8, 9);
Check("InOrderReverse", t2.InOrderReverse().Select(e => e.Key).SequenceEqual(new[] { 9, 8, 7, 5, 4, 3, 1 }));
Check("PreOrder", t2.PreOrder().Select(e => e.Key).SequenceEqual(new[] { 5, 3, 1, 4, 8, 7, 9 }));
Check("PreOrderReverse", t2.PreOrderReverse().Select(e => e.Key).SequenceEqual(new[] { 5, 8, 9, 7, 3, 4, 1 }));
Check("PostOrder", t2.PostOrder().Select(e => e.Key).SequenceEqual(new[] { 1, 4, 3, 7, 9, 8, 5 }));
Check("PostOrderReverse", t2.PostOrderReverse().Select(e => e.Key).SequenceEqual(new[] { 9, 7, 8, 4, 1, 3, 5 }));

var rootEntry = t2.InOrder().First(e => e.Key == 5);
Check("Depth корня == 0", rootEntry.Depth == 0);
var leafEntry = t2.InOrder().First(e => e.Key == 1);
Check("Depth листа 1 == 2", leafEntry.Depth == 2);

// ---------- Remove: лист ----------
var t3 = BuildTree(5, 3, 8);
Check("Remove(лист) возвращает true", t3.Remove(3));
Check("Remove(лист): ключ пропал", !t3.ContainsKey(3));
Check("Remove(лист): Count == 2", t3.Count == 2);
CheckSorted("Remove(лист): дерево валидно", t3, 5, 8);

// ---------- Remove: один ребёнок ----------
var t4 = BuildTree(5, 3, 8, 4);
Check("Remove(1 ребёнок) возвращает true", t4.Remove(3));
CheckSorted("Remove(1 ребёнок): дерево валидно", t4, 4, 5, 8);

// ---------- Remove: два ребёнка, successor - прямой потомок ----------
var t5 = BuildTree(50, 30, 70, 20, 40);
Check("Remove(2 ребёнка, successor рядом)", t5.Remove(30));
CheckSorted("Remove(2 ребёнка, successor рядом): дерево валидно", t5, 20, 40, 50, 70);

// ---------- Remove: два ребёнка, successor глубоко ----------
var t6 = BuildTree(50, 30, 70, 20, 40, 60, 80, 65, 90);
Check("Remove(корень, successor глубоко)", t6.Remove(50));
Check("Remove(корень, successor глубоко): 50 пропал", !t6.ContainsKey(50));
Check("Remove(корень, successor глубоко): Count == 8", t6.Count == 8);
CheckSorted("Remove(корень, successor глубоко): дерево валидно", t6, 20, 30, 40, 60, 65, 70, 80, 90);
Check("Remove: Count совпадает с реальным числом узлов", t6.Count == t6.InOrder().Count());

// ---------- Remove: Parent-указатели после переноса поддерева ----------
var t7 = BuildTree(50, 30, 70, 20, 40, 60, 80, 65, 90);
t7.Remove(50);
Check("Remove: повторное удаление после сложного случая (65)", t7.Remove(65));
CheckSorted("После удаления 65", t7, 20, 30, 40, 60, 70, 80, 90);
Check("Remove: повторное удаление после сложного случая (30)", t7.Remove(30));
CheckSorted("После удаления 30", t7, 20, 40, 60, 70, 80, 90);

// ---------- Remove: несуществующий ключ ----------
var t8 = BuildTree(5, 3, 8);
Check("Remove(несуществующий) == false", !t8.Remove(100));
Check("Remove(несуществующий): Count не изменился", t8.Count == 3);

// ---------- Remove: всё дерево ----------
var t9 = BuildTree(5, 3, 8, 1, 4, 7, 9);
foreach (var k in new[] { 5, 3, 8, 1, 4, 7, 9 }) t9.Remove(k);
Check("Remove: полностью пустое дерево, Count == 0", t9.Count == 0);
Check("Remove: полностью пустое дерево, InOrder пуст", !t9.InOrder().Any());

// ---------- Add/Remove вперемешку ----------
var t10 = BuildTree(50, 30, 70, 20, 40, 60, 80);
t10.Remove(30);
t10.Add(35, "v35");
t10.Remove(70);
t10.Add(75, "v75");
CheckSorted("Add/Remove вперемешку", t10, 20, 35, 40, 50, 60, 75, 80);

var t11 = BuildTree(5, 3, 8);
Check("Keys содержит все ключи", t11.Keys.OrderBy(k => k).SequenceEqual(new[] { 3, 5, 8 }));
Check("Values содержит все значения", t11.Values.Count == 3);

var arr = new KeyValuePair<int, string>[5];
t11.CopyTo(arr, 1);
Check("CopyTo: первый элемент на позиции 1", arr[1].Key == 3);

int foreachCount = 0;
foreach (var kv in t11) foreachCount++;
Check("foreach по дереву проходит все элементы", foreachCount == 3);

// ---------- Итог ----------
Console.WriteLine();
Console.WriteLine($"Пройдено: {passed}, Провалено: {failed}");