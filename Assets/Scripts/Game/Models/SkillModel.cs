using System;
using System.Collections.Generic;
using System.ComponentModel;

public class SkillModel : ObservableModel
{
    private const string Tag = "SkillModel";

    private static readonly PropertyChangedEventArgs OwnedSkillIdsChangedArgs = new PropertyChangedEventArgs(nameof(OwnedSkillIds));
    private static readonly PropertyChangedEventArgs MaxOwnedCountChangedArgs = new PropertyChangedEventArgs(nameof(MaxOwnedCount));

    private class SkillProgress
    {
        public int Level;
        public int UseCount;
    }

    private readonly List<string> _ownedSkillIds = new List<string>();
    private readonly Dictionary<string, SkillProgress> _progressById = new Dictionary<string, SkillProgress>(StringComparer.OrdinalIgnoreCase);

    private readonly List<string> _unlockCandidates = new List<string>();
    private readonly List<string> _candidatePool = new List<string>();

    private int _maxOwnedCount = 1;

    public IReadOnlyList<string> OwnedSkillIds
    {
        get { return _ownedSkillIds; }
    }

    public int MaxOwnedCount
    {
        get { return _maxOwnedCount; }
        private set { SetProperty(ref _maxOwnedCount, value, MaxOwnedCountChangedArgs); }
    }

    public bool IsFull
    {
        get { return _ownedSkillIds.Count >= _maxOwnedCount; }
    }

    public event Action<string> LevelChanged;
    public event Action<string> UseCountChanged;

    public bool Has(string skillId)
    {
        return !string.IsNullOrEmpty(skillId) && _progressById.ContainsKey(skillId);
    }

    public int GetLevel(string skillId)
    {
        return TryGetProgress(skillId, out SkillProgress progress) ? progress.Level : 0;
    }

    public int GetUseCount(string skillId)
    {
        return TryGetProgress(skillId, out SkillProgress progress) ? progress.UseCount : 0;
    }

    public void AddUse(string skillId)
    {
        if (!TryGetProgressWithData(skillId, out SkillProgress progress, out SkillData data)) { return; }

        if (data.IsMaxLevel(progress.Level)) { return; }

        int count = progress.UseCount + 1;
        bool isLevelUp = count >= data.GetRequiredUseCount(progress.Level);

        progress.UseCount = isLevelUp ? 0 : count;
        UseCountChanged?.Invoke(data.Id);

        if (isLevelUp)
        {
            LevelUp(data.Id);
        }
    }

    public bool Add(string skillId)
    {
        if (!TryAddSilently(skillId, 1, 0)) { return false; }

        _unlockCandidates.Clear();
        NotifyChanged();
        return true;
    }

    public bool Remove(string skillId)
    {
        if (!Has(skillId)) { return false; }

        _progressById.Remove(skillId);

        RemoveOwnedId(skillId);
        NotifyChanged();
        return true;
    }

    public bool LevelUp(string skillId)
    {
        if (!TryGetProgressWithData(skillId, out SkillProgress progress, out SkillData data)) { return false; }

        if (data.IsMaxLevel(progress.Level)) { return false; }

        progress.Level++;
        LevelChanged?.Invoke(data.Id);
        return true;
    }

    public void Load(IEnumerable<SkillSaveData> skills, int maxOwnedCount)
    {
        _ownedSkillIds.Clear();
        _progressById.Clear();
        _unlockCandidates.Clear();
        MaxOwnedCount = Math.Max(1, maxOwnedCount);

        if (skills != null)
        {
            foreach (SkillSaveData skill in skills)
            {
                if (skill == null) { continue; }

                TryAddSilently(skill.Id, skill.Level, skill.UseCount);
            }
        }

        NotifyChanged();
    }

    public List<SkillSaveData> CreateSaveData()
    {
        List<SkillSaveData> skills = new List<SkillSaveData>(_ownedSkillIds.Count);

        foreach (string skillId in _ownedSkillIds)
        {
            SkillProgress progress = _progressById[skillId];

            skills.Add(new SkillSaveData { Id = skillId, Level = progress.Level, UseCount = progress.UseCount });
        }

        return skills;
    }

    public void IncreaseMaxOwnedCount(int amount)
    {
        if (amount <= 0) { return; }

        MaxOwnedCount += amount;
    }

    public bool HasUnlockableSkill()
    {
        foreach (SkillData data in DataManager.Instance.Skills.All)
        {
            if (!Has(data.Id)) { return true; }
        }

        return false;
    }

    public IReadOnlyList<string> GetUnlockCandidates(int count)
    {
        _unlockCandidates.RemoveAll(Has);

        if (_unlockCandidates.Count == 0)
        {
            RollUnlockCandidates(count);
        }

        return _unlockCandidates;
    }

    private void RollUnlockCandidates(int count)
    {
        _candidatePool.Clear();

        foreach (SkillData data in DataManager.Instance.Skills.All)
        {
            if (!Has(data.Id))
            {
                _candidatePool.Add(data.Id);
            }
        }

        int pickCount = Math.Min(count, _candidatePool.Count);

        for (int i = 0; i < pickCount; i++)
        {
            int pick = UnityEngine.Random.Range(i, _candidatePool.Count);
            (_candidatePool[i], _candidatePool[pick]) = (_candidatePool[pick], _candidatePool[i]);
            _unlockCandidates.Add(_candidatePool[i]);
        }
    }

    private bool TryGetProgress(string skillId, out SkillProgress progress)
    {
        progress = null;

        if (string.IsNullOrEmpty(skillId)) { return false; }

        return _progressById.TryGetValue(skillId, out progress);
    }

    private bool TryGetProgressWithData(string skillId, out SkillProgress progress, out SkillData data)
    {
        data = null;

        if (!TryGetProgress(skillId, out progress)) { return false; }

        return DataManager.Instance.Skills.TryGetById(skillId, out data);
    }

    private bool TryAddSilently(string skillId, int level, int useCount)
    {
        if (!DataManager.Instance.Skills.TryGetById(skillId, out SkillData data))
        {
            DebugUtility.LogWarning($"[{Tag}] 스킬 테이블에 없는 Id '{skillId}'는 무시합니다.");
            return false;
        }

        if (Has(data.Id)) { return false; }

        int clampedLevel = Math.Clamp(level, 1, data.MaxLevel);
        int maxUseCount = Math.Max(0, data.GetRequiredUseCount(clampedLevel) - 1);

        _ownedSkillIds.Add(data.Id);
        _progressById.Add(data.Id, new SkillProgress { Level = clampedLevel, UseCount = Math.Clamp(useCount, 0, maxUseCount) });
        return true;
    }

    private void RemoveOwnedId(string skillId)
    {
        for (int i = _ownedSkillIds.Count - 1; i >= 0; i--)
        {
            if (string.Equals(_ownedSkillIds[i], skillId, StringComparison.OrdinalIgnoreCase))
            {
                _ownedSkillIds.RemoveAt(i);
            }
        }
    }

    private void NotifyChanged()
    {
        RaisePropertyChanged(OwnedSkillIdsChangedArgs);
    }
}
