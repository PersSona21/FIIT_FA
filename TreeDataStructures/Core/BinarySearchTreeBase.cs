using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => throw new NotImplementedException();
    public ICollection<TValue> Values => throw new NotImplementedException();
    
    
    public virtual void Add(TKey key, TValue value)
    {
        if (Root == null)
        {
            Root = CreateNode(key, value);
            OnNodeAdded(Root);
            Count++;
            return;
        }

        TNode current = Root;
        
        while (true)
        {
            int cmp = Comparer.Compare(key, current.Key);
            
            if (cmp == 0)
            {
                current.Value = value;
                return;
            }

            if (cmp < 0)
            {
                if (current.Left == null)
                {
                    TNode newNode = CreateNode(key, value);
                    newNode.Parent = current;
                    current.Left = newNode;
                    Count++;
                    OnNodeAdded(newNode);
                    return;
                }

                current = current.Left;
            }
            else
            {
                if (current.Right == null)
                {
                    TNode newNode = CreateNode(key, value);
                    newNode.Parent = current;
                    current.Right = newNode;
                    Count++;
                    OnNodeAdded(newNode);
                    return;
                }

                current = current.Right;
            }
        }
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        throw new NotImplementedException("Implement standard BST delete logic using Transplant helper");
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        TNode y = x.Right ?? throw new InvalidOperationException("Невозможно выполнить левый поворот");
        x.Right = y.Left;
        if (y.Left != null) y.Left.Parent = x;

        Transplant(x, y);
        
        // y.Parent = x.Parent;
        // if (x.Parent == null) Root = y;
        // else if (x.IsLeftChild) x.Parent.Left = y;
        // else x.Parent.Right = y;
        
        x.Parent = y;
        y.Left = x;
    }

    protected void RotateRight(TNode y)
    {
        TNode x = y.Left ?? throw new InvalidOperationException("Невозможно выполнить правый поворот");
        y.Left = x.Right;
        if (x.Right != null) x.Right.Parent = y;

        Transplant(y, x);
        
        // x.Parent = y.Parent;
        // if (y.Parent == null) Root = x;
        // else if (y.IsLeftChild) y.Parent.Left = x;
        // else y.Parent.Right = x;

        y.Parent = x;
        x.Right = y;

    }
    
    protected void RotateBigLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x.Parent ?? throw new InvalidOperationException(
            "Невозможно выполнить большой левый поворот"));
    }

    protected void RotateBigRight(TNode y)
    {
        RotateRight(y);
        RotateRight(y.Parent ?? throw new InvalidOperationException(
            "Невозможно выполнить большой правый поворот"));
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        // RL
        if (x.Right == null)
            throw new InvalidOperationException("Невозможно выполнить двойной левый поворот");

        RotateRight(x.Right);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        // LR
        if (y.Left == null)
            throw new InvalidOperationException("Невозможно выполнить двойной правый поворот");

        RotateLeft(y.Left);
        RotateRight(y);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion

    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        // probably add something here
        private TreeEntry<TKey, TValue> _current;
        private readonly TNode? _root;
        private readonly Stack<(TNode node, int depth, bool childrenPushed)> _stack; // childrenPushed для PostOrder
        private readonly TraversalStrategy _strategy;
        
        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            _stack = new Stack<(TNode, int, bool)>();
            _current = default;
            Reset();
        }
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        public TreeEntry<TKey, TValue> Current => _current;
        object IEnumerator.Current => Current;


        private void PushLeftChain(TNode? node, int depth)
        {
            while (node != null)
            {
                _stack.Push((node, depth, false));
                node = node.Left;
                depth++;
            }
        }
        
        private void PushRightChain(TNode? node, int depth)
        {
            while (node != null)
            {
                _stack.Push((node, depth, false));
                node = node.Right;
                depth++;
            }
        }
        
        public bool MoveNext()
        {
            switch (_strategy)
            {
                case TraversalStrategy.InOrder:
                {
                    if (_stack.Count == 0)
                        return false;
                    var (node, depth, _) = _stack.Pop();
                    _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                    PushLeftChain(node.Right, depth+1);
                    return true;
                }
                
                case TraversalStrategy.PreOrder:
                {
                    if (_stack.Count == 0)
                        return false;
                    var (node, depth, _) = _stack.Pop();
                    _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                
                    // Так как с начала PreOrder то с начала проходим Left потом Right
                    if (node.Right != null) _stack.Push((node.Right, depth+1, false));
                    if (node.Left != null) _stack.Push((node.Left, depth+1, false));

                    return true;
                }
                
                case TraversalStrategy.PostOrder:
                {
                    while (_stack.Count > 0)
                    {
                        var (node, depth, childrenPushed) = _stack.Pop();

                        if (childrenPushed)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            return true;
                        }
                    
                        _stack.Push((node, depth, true));
                        if (node.Right != null) _stack.Push((node.Right, depth + 1, false));
                        if (node.Left  != null) _stack.Push((node.Left,  depth + 1, false));
                    }
                    return false;
                }

                case TraversalStrategy.InOrderReverse:
                {
                    if (_stack.Count == 0)
                        return false;

                    var (node, depth, _) = _stack.Pop();
                    _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                    PushRightChain(node.Left, depth+1);
                    return true;
                }

                case TraversalStrategy.PreOrderReverse:
                {
                    if (_stack.Count == 0)
                        return false;

                    var (node, depth, _) = _stack.Pop();
                    _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                    if (node.Left != null) _stack.Push((node.Left, depth+1,false));
                    if (node.Right != null) _stack.Push((node.Right, depth+1,false));
                    return true;
                }

                case TraversalStrategy.PostOrderReverse:
                {
                    while (_stack.Count > 0)
                    {
                        var (node, depth, childrenPushed) = _stack.Pop();

                        if (childrenPushed)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            return true;
                        }
                        
                        _stack.Push((node, depth, true));
                        if (node.Left != null) _stack.Push((node.Left, depth+1, false));
                        if (node.Right != null) _stack.Push((node.Right, depth+1, false));
                        
                    }

                    return false;
                }
                    
                default:
                    throw new NotImplementedException("Strategy not implemented");
            }
        }
        
        public void Reset()
        {
            _stack.Clear();

            switch (_strategy)
            {
                case TraversalStrategy.InOrder:
                    PushLeftChain(_root, 0);
                    break;
                
                case TraversalStrategy.InOrderReverse:
                    PushRightChain(_root, 0);
                    break;
                
                case TraversalStrategy.PreOrder:
                case TraversalStrategy.PostOrder:
                case TraversalStrategy.PreOrderReverse:
                case TraversalStrategy.PostOrderReverse:    
                    if (_root != null) _stack.Push((_root, 0, false));
                    break;
                
                default:
                    throw new NotImplementedException("Strategy not implemented");
            }
        }
        
        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}