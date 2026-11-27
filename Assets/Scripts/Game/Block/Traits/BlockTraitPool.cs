using System.Collections.Generic;
using UnityEngine;

public static class BlockTraitPool
{
    private const string Tag = "BlockTrait";

    private static readonly Dictionary<BlockTraitType, Stack<BlockTrait>> Pools = new Dictionary<BlockTraitType, Stack<BlockTrait>>();

    public static BlockTrait Get(Block owner, BlockTraitData data)
    {
        if (data == null)
        {
            DebugUtility.LogError($"[{Tag}] 특성 데이터가 비어 있습니다.");
            return null;
        }

        BlockTrait trait = PopOrCreate(data.Type);

        if (trait == null)
        {
            DebugUtility.LogError($"[{Tag}] 알 수 없는 특성입니다. ({data.Type})");
            return null;
        }

        trait.Initialize(owner, data);
        return trait;
    }

    public static void Release(BlockTrait trait)
    {
        if (trait == null) { return; }

        trait.Reset();

        if (!Pools.TryGetValue(trait.Type, out Stack<BlockTrait> pool))
        {
            pool = new Stack<BlockTrait>();
            Pools.Add(trait.Type, pool);
        }

        pool.Push(trait);
    }

    private static BlockTrait PopOrCreate(BlockTraitType type)
    {
        if (Pools.TryGetValue(type, out Stack<BlockTrait> pool))
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
        }

        BlockTraitFactory.TryCreate(type, out BlockTrait trait);
        return trait;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPools()
    {
        Pools.Clear();
    }
}
