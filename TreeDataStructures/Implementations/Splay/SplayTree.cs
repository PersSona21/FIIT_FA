using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    public override bool ContainsKey(TKey key) => TryGetValue(key, out _);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (parent != null) Splay(parent);
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var current = Root;
        BstNode<TKey, TValue>? last = null;

        while (current != null)
        {
            last = current;
            int cmp = Comparer.Compare(key, current.Key);

            if (cmp == 0)
            {
                value = current.Value;
                Splay(current);
                return true;
            }
            current = cmp < 0 ? current.Left : current.Right;
        }
        
        if (last != null) Splay(last);

        value = default;
        return false;
    }

    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null)
        {
            
            BstNode<TKey, TValue> parent = node.Parent;
            BstNode<TKey, TValue>? grandparent = parent.Parent;
            
            if (grandparent == null)
            {
                // Zig - один поворот
                if (node.IsLeftChild) RotateRight(parent);
                else RotateLeft(parent);
            }
            else if (node.IsLeftChild == parent.IsLeftChild)
            {
                // Zig-zig - двойной поворот
                if (node.IsLeftChild) RotateDoubleRight(grandparent);
                else RotateDoubleLeft(grandparent);
            }
            else
            {
                // Zig-zag - большой поворот
                if (node.IsLeftChild) RotateBigLeft(grandparent);
                else RotateBigRight(grandparent);
            }
        }
    }
    
}
