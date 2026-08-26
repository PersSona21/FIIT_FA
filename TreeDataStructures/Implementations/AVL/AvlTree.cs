using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    #region Height / Balance helpers
    
    private static int GetHeight(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;
    
    private static void UpdateHeight(AvlNode<TKey, TValue> node)
        => node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
    
    private static int GetBalanceFactor(AvlNode<TKey, TValue> node)
        => GetHeight(node.Left) - GetHeight(node.Right);

    #endregion

    /// <summary>
    /// Приводит поддерево с вершиной <paramref name="node"/> к балансу (если нужно)
    /// и возвращает новую вершину этого поддерева (может совпасть с исходной,
    /// если поворот не потребовался).
    /// </summary>
    private AvlNode<TKey, TValue> BalanceNode(AvlNode<TKey, TValue> node)
    {
        UpdateHeight(node);
        int bf = GetBalanceFactor(node);

        if (bf > 1)
        {
            if (GetBalanceFactor(node.Left!) < 0)
            {
                RotateBigRight(node); // LR
            }
            else
            {
                RotateRight(node);
            }
            
            AvlNode<TKey, TValue> newTop = node.Parent!;
            UpdateHeight(node);
            UpdateHeight(newTop); // новый корень
            return newTop;
        }

        if (bf < -1)
        {
            if (GetBalanceFactor(node.Right!) > 0)
            {
                RotateBigLeft(node); // RL
            }
            else
            {
                RotateLeft(node);
            }
            
            AvlNode<TKey, TValue> newTop = node.Parent!;
            UpdateHeight(node);
            UpdateHeight(newTop); // новый корень
            return newTop;
        }

        return node;
    }

    /// <summary>
    /// Поднимается от <paramref name="node"/> к корню, восстанавливая
    /// АВЛ-инвариант на каждом уровне.
    /// </summary>
    protected void Rebalance(AvlNode<TKey, TValue>? node)
    {
        while (node != null)
        {
            node = BalanceNode(node).Parent;
        }

    }
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        Rebalance(newNode.Parent);
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        Rebalance(parent);
    }
}