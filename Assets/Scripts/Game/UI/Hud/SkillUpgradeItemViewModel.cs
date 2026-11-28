using System;
using System.ComponentModel;
using UnityEngine;

public class SkillUpgradeItemViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly PropertyChangedEventArgs LevelChangedArgs = new PropertyChangedEventArgs(nameof(Level));
    private static readonly PropertyChangedEventArgs CanUpgradeChangedArgs = new PropertyChangedEventArgs(nameof(CanUpgrade));
    private static readonly PropertyChangedEventArgs UseProgressChangedArgs = new PropertyChangedEventArgs(nameof(UseProgress));
    private static readonly PropertyChangedEventArgs DpsTextChangedArgs = new PropertyChangedEventArgs(nameof(DpsText));

    private readonly SkillData _data;

    private SkillModel _skillModel;
    private CurrencyModel _currencyModel;
    private PlayerModel _playerModel;
    private ISkillStatsModel _stats;

    private bool _canUpgrade;

    public event PropertyChangedEventHandler PropertyChanged;

    public string SkillId { get; }

    public Sprite Icon
    {
        get
        {
            if (_data == null)
            {
                return null;
            }

            return _data.IconSprite;
        }
    }

    public string Name
    {
        get
        {
            if (_data == null)
            {
                return SkillId;
            }

            return _data.Name;
        }
    }

    public int Level
    {
        get
        {
            if (_skillModel == null)
            {
                return 0;
            }

            return _skillModel.GetLevel(SkillId);
        }
    }

    public bool IsMaxLevel
    {
        get { return _data == null || _data.IsMaxLevel(Level); }
    }

    public double UpgradeCost
    {
        get
        {
            if (_data == null)
            {
                return 0;
            }

            return _data.GetUpgradeCost(Level);
        }
    }

    public float UseProgress
    {
        get
        {
            if (IsMaxLevel) { return 1f; }

            int required = _data.GetRequiredUseCount(Level);

            if (required <= 0 || _skillModel == null)
            {
                return 0f;
            }

            return (float)_skillModel.GetUseCount(SkillId) / required;
        }
    }

    public bool CanUpgrade
    {
        get { return _canUpgrade; }
    }

    public float CooldownRatio
    {
        get
        {
            if (_skillModel == null)
            {
                return 0f;
            }

            return _skillModel.GetCooldownRatio(SkillId, Time.time);
        }
    }

    public double DamagePerSecond
    {
        get
        {
            if (_skillModel == null || _playerModel == null)
            {
                return 0;
            }

            return SkillDamageCalculator.GetDamagePerSecond(_playerModel.AttackPower, _stats, _data, Level);
        }
    }

    public string DpsText
    {
        get { return $"DPS {NumberUtility.Format(DamagePerSecond)}/s"; }
    }

    public string LevelText
    {
        get { return $"Lv. {Level}"; }
    }

    public string CostText
    {
        get
        {
            if (IsMaxLevel)
            {
                return "MAX";
            }

            return NumberUtility.Format(UpgradeCost);
        }
    }

    public SkillUpgradeItemViewModel(string skillId)
    {
        SkillId = skillId;
        DataManager.Instance.Skills.TryGetById(skillId, out _data);

        _skillModel = ModelManager.Instance.SkillModel;
        _currencyModel = ModelManager.Instance.CurrencyModel;
        _playerModel = ModelManager.Instance.PlayerModel;

        _playerModel.PropertyChanged += OnPlayerModelPropertyChanged;

        if (ModelManager.Instance.TryGetSkillStats(skillId, out _stats))
        {
            _stats.PropertyChanged += OnStatsPropertyChanged;
        }

        _skillModel.LevelChanged += OnLevelChanged;
        _skillModel.UseCountChanged += OnUseCountChanged;
        _currencyModel.PropertyChanged += OnCurrencyModelPropertyChanged;

        _canUpgrade = EvaluateCanUpgrade();
    }

    public void Dispose()
    {
        if (_skillModel != null)
        {
            _skillModel.LevelChanged -= OnLevelChanged;
            _skillModel.UseCountChanged -= OnUseCountChanged;
            _skillModel = null;
        }

        if (_currencyModel != null)
        {
            _currencyModel.PropertyChanged -= OnCurrencyModelPropertyChanged;
            _currencyModel = null;
        }

        if (_playerModel != null)
        {
            _playerModel.PropertyChanged -= OnPlayerModelPropertyChanged;
            _playerModel = null;
        }

        if (_stats != null)
        {
            _stats.PropertyChanged -= OnStatsPropertyChanged;
            _stats = null;
        }

        _canUpgrade = false;
    }

    public bool Upgrade()
    {
        if (_skillModel == null || !EvaluateCanUpgrade()) { return false; }

        double cost = UpgradeCost;

        if (!_currencyModel.TrySpendGold(cost)) { return false; }

        if (!_skillModel.LevelUp(SkillId))
        {
            _currencyModel.AddGold(cost);
            return false;
        }

        return true;
    }

    private void OnLevelChanged(string skillId)
    {
        if (!string.Equals(skillId, SkillId, StringComparison.OrdinalIgnoreCase)) { return; }

        PropertyChanged?.Invoke(this, LevelChangedArgs);
        PropertyChanged?.Invoke(this, UseProgressChangedArgs);
        PropertyChanged?.Invoke(this, DpsTextChangedArgs);
        RefreshCanUpgrade();
    }

    private void OnPlayerModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PlayerModel.AttackPower):
                PropertyChanged?.Invoke(this, DpsTextChangedArgs);
                break;
        }
    }

    private void OnStatsPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISkillStatsModel.Cooldown):
            case nameof(ISkillStatsModel.AttackPowerMultiplier):
                PropertyChanged?.Invoke(this, DpsTextChangedArgs);
                break;
        }
    }

    private void OnUseCountChanged(string skillId)
    {
        if (!string.Equals(skillId, SkillId, StringComparison.OrdinalIgnoreCase)) { return; }

        PropertyChanged?.Invoke(this, UseProgressChangedArgs);
    }

    private void OnCurrencyModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CurrencyModel.Gold):
                RefreshCanUpgrade();
                break;
        }
    }

    private void RefreshCanUpgrade()
    {
        bool canUpgrade = EvaluateCanUpgrade();

        if (canUpgrade == _canUpgrade) { return; }

        _canUpgrade = canUpgrade;
        PropertyChanged?.Invoke(this, CanUpgradeChangedArgs);
    }

    private bool EvaluateCanUpgrade()
    {
        return _data != null && _skillModel != null && _skillModel.Has(SkillId) && !IsMaxLevel && _currencyModel.Gold >= UpgradeCost;
    }
}
