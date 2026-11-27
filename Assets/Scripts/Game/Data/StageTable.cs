using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class StageTable
{
    private const string Tag = "StageTable";
    private const string UnitName = "스테이지";

    private readonly Dictionary<int, StageData> _dataByStage = new Dictionary<int, StageData>();

    public async UniTask LoadAsync(CancellationToken token)
    {
        _dataByStage.Clear();

        List<StageData> rows = await JsonTableLoader.LoadAsync<StageData>(AddressKeys.StageTable, Tag, DescribeRow, token);

        foreach (StageData data in rows)
        {
            TryRegister(data);
        }

        TableChecks.CheckSequential(_dataByStage.Keys, rows.Count, Tag, UnitName);
    }

    public void CheckLayoutIds(BlockLayoutTable layoutTable)
    {
        foreach (StageData data in _dataByStage.Values)
        {
            if (layoutTable.TryGetById(data.LayoutId, out _)) { continue; }

            DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} LayoutId '{data.LayoutId}' 레이아웃이 없습니다.");
        }
    }

    public StageData Get(int stage)
    {
        if (!TryGet(stage, out StageData data))
        {
            DebugUtility.LogError($"[{Tag}] {stage}스테이지 데이터가 없습니다.");
            return null;
        }

        return data;
    }

    public bool TryGet(int stage, out StageData data)
    {
        return _dataByStage.TryGetValue(stage, out data);
    }

    private void TryRegister(StageData data)
    {
        if (!_dataByStage.TryAdd(data.Stage, data))
        {
            DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} 제외됨: 스테이지 중복");
        }
    }

    private static string DescribeRow(StageData data)
    {
        return $"{data.Stage}스테이지";
    }
}
