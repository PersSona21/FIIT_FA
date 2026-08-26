using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null) return (null, null);
        
        var cmp = Comparer.Compare(root.Key, key);
        if (cmp <= 0)
        {
            var (left, right) = Split(root.Right, key);
            root.Right = left;
            root.Right?.Parent = root;
            return (root, right);
        }
        else
        {
            var (left, right) = Split(root.Left, key);
            root.Left = right;
            root.Left?.Parent = root;
            return (left, root);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (right == null) return left;
        if (left == null) return right;
        
        if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            left.Right?.Parent = left;
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
            right.Left?.Parent = right;
            return right;
        }
    }
    

    public override void Add(TKey key, TValue value)
    {
        var exist = FindNode(key);
        if (exist != null)
        {
            exist.Value = value;
            return;
        }

        var newNode = CreateNode(key, value);
        var (left, right) = Split(this.Root, key);
        // сливаем левое дерево с newNode, сливаем получившееся дерево с правым деревом
        Root = Merge(Merge(left, newNode), right);
        Count++;
        OnNodeAdded(newNode);
    }

    public override bool Remove(TKey key)
    {
        // ищем ноду 
        var node = FindNode(key);
        if (node == null) return false;

        var parent = node.Parent;
        // merge его левого и правого сына
        var merged = Merge(node.Left, node.Right);
        // меняем ноду на результат merge
        Transplant(node, merged);

        Count--;
        OnNodeRemoved(parent, merged);
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }

    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode){ }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child) { }
    
}