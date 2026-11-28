using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SkillTable
{
    private const string Tag = "SkillTable";
    private const string UnitName = "스킬";

    private readonly Dictionary<string, SkillData> _dataById = new Dictionary<string, SkillData>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<SkillData> All
    {
        get { return _dataById.Values; }
    }

    public async UniTask LoadAsync(CancellationToken token)
    {
        _dataById.Clear();

        List<SkillData> rows = await JsonTableLoader.LoadAsync<SkillData>(AddressKeys.Data.SkillTable, Tag, DescribeRow, token);

        RegisterRows(rows);
        CheckStartingSkill();
        await LoadIconsAsync(token);
    }

    public SkillData GetById(string id)
    {
        if (!TryGetById(id, out SkillData data))
        {
            DebugUtility.LogError($"[{Tag}] Id '{id}' 데이터가 없습니다.");
            return null;
        }

        return data;
    }

    public bool TryGetById(string id, out SkillData data)
    {
        data = null;

        if (string.IsNullOrEmpty(id)) { return false; }

        return _dataById.TryGetValue(id, out data);
    }

    private void RegisterRows(List<SkillData> rows)
    {
        foreach (SkillData data in rows)
        {
            TryRegister(data);
        }

        TableChecks.CheckNotEmpty(_dataById.Count, rows.Count, Tag, UnitName);
    }

    private void TryRegister(SkillData data)
    {
        if (!_dataById.TryAdd(data.Id, data))
        {
            DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} 제외됨: Id 중복");
        }
    }

    private void CheckStartingSkill()
    {
        if (_dataById.ContainsKey(SkillIds.SwordAttack)) { return; }

        DebugUtility.LogError($"[{Tag}] 시작 스킬 '{SkillIds.SwordAttack}'이 테이블에 없습니다.");
    }

    private UniTask LoadIconsAsync(CancellationToken token)
    {
        List<UniTask> loadTasks = new List<UniTask>(_dataById.Count);

        foreach (SkillData data in _dataById.Values)
        {
            loadTasks.Add(LoadIconAsync(data, token));
        }

        return UniTask.WhenAll(loadTasks);
    }

    private static async UniTask LoadIconAsync(SkillData data, CancellationToken token)
    {
        if (string.IsNullOrEmpty(data.Icon)) { return; }

        string address = data.IconAddress;

        try
        {
            data.IconSprite = await Addressables.LoadAssetAsync<Sprite>(address).ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            data.IconSprite = null;
            DebugUtility.LogWarning($"[{Tag}] {DescribeRow(data)}: Addressable '{address}' 아이콘 로드 실패. {exception.Message}");
            return;
        }

        if (data.IconSprite == null)
        {
            DebugUtility.LogWarning($"[{Tag}] {DescribeRow(data)}: Addressable '{address}' 아이콘이 없습니다.");
        }
    }

    private static string DescribeRow(SkillData data)
    {
        return $"스킬 '{data.Id}'";
    }
}
