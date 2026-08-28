using System.Runtime.CompilerServices;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public enum RbColor : byte
{
    Red   = 0,
    Black = 1
}

public class RbNode<TKey, TValue>(TKey key, TValue value)
    : Node<TKey, TValue, RbNode<TKey, TValue>>(key, value)
{
    public RbColor Color { get; set; } = RbColor.Red;
    public RbNode<TKey, TValue>? Grandparent => this.Parent?.Parent;
    public RbNode<TKey, TValue>? Uncle => this.Parent is { IsLeftChild: true } ? this.Grandparent?.Right : this.Grandparent?.Left;
    public RbNode<TKey, TValue>? Sibling => this.IsLeftChild ? this.Parent?.Right : this.Parent?.Left;
}