using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ChapterTable
{
    private const string Tag = "ChapterTable";
    private const string UnitName = "챕터";

    private readonly Dictionary<int, ChapterData> _dataByChapter = new Dictionary<int, ChapterData>();

    public async UniTask LoadAsync(CancellationToken token)
    {
        _dataByChapter.Clear();

        List<ChapterData> rows = await JsonTableLoader.LoadAsync<ChapterData>(AddressKeys.ChapterTable, Tag, DescribeRow, token);

        foreach (ChapterData data in rows)
        {
            TryRegister(data);
        }

        TableChecks.CheckSequential(_dataByChapter.Keys, rows.Count, Tag, UnitName);
    }

    public void CheckBlockIds(BlockTable blockTable)
    {
        foreach (ChapterData data in _dataByChapter.Values)
        {
            for (int i = 0; i < data.Traits.Count; i++)
            {
                CheckTraitBlockIds(data, i, blockTable);
            }
        }
    }

    public ChapterData Get(int chapter)
    {
        if (!TryGet(chapter, out ChapterData data))
        {
            DebugUtility.LogError($"[{Tag}] {chapter}챕터 데이터가 없습니다.");
            return null;
        }

        return data;
    }

    public bool TryGet(int chapter, out ChapterData data)
    {
        return _dataByChapter.TryGetValue(chapter, out data);
    }

    private void TryRegister(ChapterData data)
    {
        if (!_dataByChapter.TryAdd(data.Chapter, data))
        {
            DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} 제외됨: Chapter 중복");
            return;
        }

        RemoveInvalidTraits(data);
    }

    private static void RemoveInvalidTraits(ChapterData data)
    {
        List<string> errors = data.RemoveInvalidTraits();

        if (errors.Count == 0) { return; }

        DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} 특성 제외됨: {string.Join(", ", errors)}");
    }

    private static void CheckTraitBlockIds(ChapterData data, int traitIndex, BlockTable blockTable)
    {
        List<string> blockIds = data.Traits[traitIndex].BlockIds;

        if (blockIds == null) { return; }

        foreach (string blockId in blockIds)
        {
            if (blockTable.TryGetById(blockId, out _)) { continue; }

            DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} Traits[{traitIndex}] BlockIds의 '{blockId}' 블록이 없습니다.");
        }
    }

    private static string DescribeRow(ChapterData data)
    {
        return $"{data.Chapter}챕터";
    }
}
