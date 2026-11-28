using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class PlayerSkillsViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly PropertyChangedEventArgs OwnedSkillIdsChangedArgs = new PropertyChangedEventArgs(nameof(OwnedSkillIds));

    private static readonly IReadOnlyList<string> EmptySkillIds = new List<string>();

    private SkillModel _model;

    public event PropertyChangedEventHandler PropertyChanged;

    public IReadOnlyList<string> OwnedSkillIds
    {
        get
        {
            if (_model == null)
            {
                return EmptySkillIds;
            }

            return _model.OwnedSkillIds;
        }
    }

    public PlayerSkillsViewModel()
    {
        _model = ModelManager.Instance.SkillModel;
        _model.PropertyChanged += OnModelPropertyChanged;
    }

    public void Dispose()
    {
        if (_model != null)
        {
            _model.PropertyChanged -= OnModelPropertyChanged;
            _model = null;
        }
    }

    public bool Has(string skillId)
    {
        return _model != null && _model.Has(skillId);
    }

    public void RecordUse(string skillId, float cooldown)
    {
        if (_model == null) { return; }

        _model.AddUse(skillId, Time.time, cooldown);
    }

    public bool TryGetPrefabAddress(string skillId, out string address)
    {
        address = null;

        if (!DataManager.Instance.Skills.TryGetById(skillId, out SkillData data)) { return false; }

        if (string.IsNullOrWhiteSpace(data.Prefab)) { return false; }

        address = data.PrefabAddress;
        return true;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SkillModel.OwnedSkillIds):
                PropertyChanged?.Invoke(this, OwnedSkillIdsChangedArgs);
                break;
        }
    }
}
