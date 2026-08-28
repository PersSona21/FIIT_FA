using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value) => new(key, value);

    #region Color helpers
    
    protected void SetBlack(RbNode<TKey, TValue>? node)
    {
        node?.Color = RbColor.Black;
    }
    
    protected void SetRed(RbNode<TKey, TValue>? node)
    {
        node?.Color = RbColor.Red;
    }

    protected RbColor GetColor(RbNode<TKey, TValue>? node) => node?.Color ?? RbColor.Black;
    
    protected bool IsBlack(RbNode<TKey, TValue>? node) => GetColor(node) == RbColor.Black;
    
    protected bool IsRed(RbNode<TKey, TValue>? node) => GetColor(node) == RbColor.Red;
    #endregion
    
    private static RbNode<TKey, TValue> FindMin(RbNode<TKey, TValue> node)
    {
        while (node.Left != null) node = node.Left;
        return node;
    }
    
    private static RbNode<TKey, TValue>? SiblingOf(RbNode<TKey, TValue>? node, RbNode<TKey, TValue> parent)
        => node == parent.Left ? parent.Right : parent.Left;

    #region Insert
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        FixInsert(newNode);
    }

    private void FixInsert(RbNode<TKey, TValue> node)
    {
        while (IsRed(node.Parent))
        {
            var parent = node.Parent!;
            var grandparent = node.Grandparent!;
            var uncle = node.Uncle;

            if (parent.IsLeftChild)
            {
                if (IsRed(uncle))
                {
                    SetBlack(parent);
                    SetBlack(uncle);
                    SetRed(grandparent);
                    node = grandparent;
                }
                else
                {
                    if (node.IsRightChild)
                    {
                        node = parent;
                        RotateLeft(node);
                    }
                    parent = node.Parent!;
                    grandparent = node.Grandparent!;
                    SetBlack(parent);
                    SetRed(grandparent);
                    RotateRight(grandparent);
                }
            }
            else
            {
                if (IsRed(uncle))
                {
                    SetBlack(parent);
                    SetBlack(uncle);
                    SetRed(grandparent);
                    node = grandparent;
                }
                else
                {
                    if (node.IsLeftChild)
                    {
                        node = parent;
                        RotateRight(node);
                    }
                    parent = node.Parent!;
                    grandparent = node.Grandparent!;
                    SetBlack(parent);
                    SetRed(grandparent);
                    RotateLeft(grandparent);
                }
            }
        }
        SetBlack(this.Root);
    }
    
    #endregion
    
    #region Remove
    
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        
    }

    protected override void RemoveNode(RbNode<TKey, TValue> node)
    {
        RbNode<TKey, TValue> y = node;  // узел, который вырезается
        RbColor yOriginalColor = y.Color;

        RbNode<TKey, TValue>? x;  // кто занял вырезанное место
        RbNode<TKey, TValue>? xParent;

        if (node.Left == null)
        {
            x = node.Right;
            xParent = node.Parent;
            Transplant(node, node.Right);
        }
        else if (node.Right == null)
        {
            x = node.Left;
            xParent = node.Parent;
            Transplant(node, node.Left);
        }
        else
        {
            y = FindMin(node.Right);
            yOriginalColor = y.Color;
            x = y.Right;

            if (y.Parent == node)
            {
                xParent = y;
            }
            else
            {
                xParent = y.Parent;
                Transplant(y, y.Right);
                y.Right = node.Right;
                y.Right.Parent = y;
            }

            Transplant(node, y);
            y.Left = node.Left;
            y.Left.Parent = y;
            y.Color = node.Color;
        }

        if (yOriginalColor == RbColor.Black)
        {
            FixDelete(x, xParent);
        }
    }
    private void FixDelete(RbNode<TKey, TValue>? x, RbNode<TKey, TValue>? xParent)
    {
        while (x != Root && IsBlack(x))
        {
            if (x == xParent!.Left)
            {
                var sibling = SiblingOf(x, xParent);

                if (IsRed(sibling))
                {
                    SetBlack(sibling);
                    SetRed(xParent);
                    RotateLeft(xParent);
                    sibling = xParent.Right;
                }

                if (IsBlack(sibling?.Left) && IsBlack(sibling?.Right))
                {
                    SetRed(sibling);
                    x = xParent;
                    xParent = x.Parent;
                }
                else
                {
                    if (IsBlack(sibling?.Right))
                    {
                        SetBlack(sibling!.Left);
                        SetRed(sibling);
                        RotateRight(sibling);
                        sibling = xParent.Right;
                    }

                    if (sibling != null) sibling.Color = xParent.Color;
                    SetBlack(xParent);
                    SetBlack(sibling?.Right);
                    RotateLeft(xParent);
                    x = Root;
                    xParent = null;
                }
            }
            else // зеркально
            {
                var sibling = SiblingOf(x, xParent);

                if (IsRed(sibling))
                {
                    SetBlack(sibling);
                    SetRed(xParent);
                    RotateRight(xParent);
                    sibling = xParent.Left;
                }

                if (IsBlack(sibling?.Right) && IsBlack(sibling?.Left))
                {
                    SetRed(sibling);
                    x = xParent;
                    xParent = x.Parent;
                }
                else
                {
                    if (IsBlack(sibling?.Left))
                    {
                        SetBlack(sibling!.Right);
                        SetRed(sibling);
                        RotateLeft(sibling);
                        sibling = xParent.Left;
                    }

                    if (sibling != null) sibling.Color = xParent.Color;
                    SetBlack(xParent);
                    SetBlack(sibling?.Left);
                    RotateRight(xParent);
                    x = Root;
                    xParent = null;
                }
            }
        }

        SetBlack(x);
    }
    
    #endregion
}