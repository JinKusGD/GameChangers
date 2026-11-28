using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerSkillSpawner : MonoBehaviour
{
    private const string Tag = "PlayerSkillSpawner";

    [SerializeField] private Transform _skillRoot;

    private readonly Dictionary<string, Skill> _skillsById = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Skill, string> _idsBySkill = new Dictionary<Skill, string>();

    private readonly HashSet<string> _loadingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _releaseBuffer = new List<string>();

    private PlayerSkillsViewModel _viewModel;

    private void Awake()
    {
        enabled = UnityUtility.CheckReference(_skillRoot, nameof(_skillRoot), this);
    }

    private void Start()
    {
        if (!enabled) { return; }

        _viewModel = new PlayerSkillsViewModel();
        _viewModel.PropertyChanged += OnPropertyChanged;
        ApplyOwnedSkills();
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged;
            _viewModel.Dispose();
            _viewModel = null;
        }

        foreach (Skill skill in _skillsById.Values)
        {
            ReleaseSkill(skill);
        }

        _skillsById.Clear();
        _idsBySkill.Clear();
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.OwnedSkillIds):
                ApplyOwnedSkills();
                break;
        }
    }

    private void ApplyOwnedSkills()
    {
        ReleaseUnownedSkills();
        SpawnMissingSkills();
    }

    private void ReleaseUnownedSkills()
    {
        _releaseBuffer.Clear();

        foreach (string skillId in _skillsById.Keys)
        {
            if (!_viewModel.Has(skillId))
            {
                _releaseBuffer.Add(skillId);
            }
        }

        foreach (string skillId in _releaseBuffer)
        {
            Skill skill = _skillsById[skillId];

            _skillsById.Remove(skillId);
            _idsBySkill.Remove(skill);
            ReleaseSkill(skill);
        }
    }

    private void SpawnMissingSkills()
    {
        foreach (string skillId in _viewModel.OwnedSkillIds)
        {
            if (_skillsById.ContainsKey(skillId) || _loadingIds.Contains(skillId)) { continue; }

            SpawnAsync(skillId).Forget();
        }
    }

    private async UniTaskVoid SpawnAsync(string skillId)
    {
        if (!_viewModel.TryGetPrefabAddress(skillId, out string address))
        {
            DebugUtility.LogError($"[{Tag}] 스킬 '{skillId}'의 프리팹 주소를 찾지 못했습니다.", this);
            return;
        }

        _loadingIds.Add(skillId);

        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, _skillRoot, false);
        GameObject instance = null;

        try
        {
            instance = await handle.ToUniTask();
        }
        catch (Exception e)
        {
            DebugUtility.LogError($"[{Tag}] 스킬 프리팹 '{address}'을(를) 생성하지 못했습니다: {e.Message}", this);
        }
        finally
        {
            _loadingIds.Remove(skillId);
        }

        if (instance == null)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return;
        }

        if (this == null || _viewModel == null || !_viewModel.Has(skillId))
        {
            ReleaseInstance(instance);
            return;
        }

        if (!instance.TryGetComponent(out Skill skill))
        {
            DebugUtility.LogError($"[{Tag}] 스킬 프리팹 '{address}'에 Skill 컴포넌트가 없습니다.", this);
            ReleaseInstance(instance);
            return;
        }

        _skillsById.Add(skillId, skill);
        _idsBySkill.Add(skill, skillId);
        skill.Used += OnSkillUsed;
    }

    private void OnSkillUsed(Skill skill, Block target)
    {
        if (_viewModel == null) { return; }

        if (!_idsBySkill.TryGetValue(skill, out string skillId)) { return; }

        _viewModel.RecordUse(skillId, skill.CooldownRemaining);
    }

    private void ReleaseSkill(Skill skill)
    {
        if (skill == null) { return; }

        skill.Used -= OnSkillUsed;
        ReleaseInstance(skill.gameObject);
    }

    private static void ReleaseInstance(GameObject instance)
    {
        if (instance == null) { return; }

        if (!Addressables.ReleaseInstance(instance))
        {
            Destroy(instance);
        }
    }
}
