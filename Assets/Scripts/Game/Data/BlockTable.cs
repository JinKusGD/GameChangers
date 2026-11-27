using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class BlockTable
{
    private const string Tag = "BlockTable";
    private const string UnitName = "블록";

    private readonly Dictionary<string, BlockData> _dataById = new Dictionary<string, BlockData>(StringComparer.OrdinalIgnoreCase);
    private readonly BlockData[] _dataByKey = new BlockData[BlockKey.Max + 1];

    public async UniTask LoadAsync(CancellationToken token)
    {
        Clear();

        List<BlockData> rows = await JsonTableLoader.LoadAsync<BlockData>(AddressKeys.BlockTable, Tag, DescribeRow, token);

        RegisterRows(rows);
        CheckSplitTargets();
        await LoadSpritesAsync(token);
    }

    public BlockData GetById(string id)
    {
        if (!TryGetById(id, out BlockData data))
        {
            DebugUtility.LogError($"[{Tag}] Id '{id}' 데이터가 없습니다.");
            return null;
        }

        return data;
    }

    public bool TryGetById(string id, out BlockData data)
    {
        data = null;

        if (string.IsNullOrEmpty(id)) { return false; }

        return _dataById.TryGetValue(id, out data);
    }

    public BlockData GetByKey(int key)
    {
        if (!BlockKey.IsValid(key) || _dataByKey[key] == null)
        {
            DebugUtility.LogError($"[{Tag}] Key {key} 데이터가 없습니다.");
            return null;
        }

        return _dataByKey[key];
    }

    private void Clear()
    {
        _dataById.Clear();
        Array.Clear(_dataByKey, 0, _dataByKey.Length);
    }

    private void RegisterRows(List<BlockData> rows)
    {
        foreach (BlockData data in rows)
        {
            TryRegister(data);
        }

        TableChecks.CheckNotEmpty(_dataById.Count, rows.Count, Tag, UnitName);
    }

    private void TryRegister(BlockData data)
    {
        if (_dataByKey[data.Key] != null)
        {
            DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} 제외됨: Key {data.Key} 중복");
            return;
        }

        if (!_dataById.TryAdd(data.Id, data))
        {
            DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} 제외됨: Id 중복");
            return;
        }

        _dataByKey[data.Key] = data;
    }

    private void CheckSplitTargets()
    {
        List<BlockTraitData> invalidTraits = new List<BlockTraitData>();

        foreach (BlockData data in _dataById.Values)
        {
            BlockTraitData split = FindSplitTrait(data);

            if (split == null) { continue; }

            if (!_dataById.ContainsKey(split.BlockId))
            {
                DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} Split 대상 '{split.BlockId}' 블록이 없어 분열하지 않습니다.");
                invalidTraits.Add(split);
            }
            else if (IsSplitCycle(data))
            {
                DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} Split 대상이 자기 자신으로 돌아와 분열하지 않습니다. (대상 '{split.BlockId}')");
                invalidTraits.Add(split);
            }
        }

        foreach (BlockTraitData trait in invalidTraits)
        {
            trait.BlockId = string.Empty;
        }
    }

    private bool IsSplitCycle(BlockData start)
    {
        HashSet<BlockData> visited = new HashSet<BlockData> { start };
        BlockData current = start;

        while (true)
        {
            BlockTraitData split = FindSplitTrait(current);

            if (split == null || !_dataById.TryGetValue(split.BlockId, out BlockData next)) { return false; }

            if (next == start) { return true; }

            if (!visited.Add(next)) { return false; }

            current = next;
        }
    }

    private static BlockTraitData FindSplitTrait(BlockData data)
    {
        foreach (BlockTraitData trait in data.Traits)
        {
            if (trait.Type == BlockTraitType.Split) { return trait; }
        }

        return null;
    }

    private UniTask LoadSpritesAsync(CancellationToken token)
    {
        List<UniTask> loadTasks = new List<UniTask>(_dataById.Count);

        foreach (BlockData data in _dataById.Values)
        {
            loadTasks.Add(data.LoadSpriteAsync(token));
        }

        return UniTask.WhenAll(loadTasks);
    }

    private static string DescribeRow(BlockData data)
    {
        return $"'{data.Id}'";
    }
}
