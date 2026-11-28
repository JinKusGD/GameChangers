using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeItemView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _cooldown;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _dpsText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Image _progressFill;
    [SerializeField] private Button _upgradeButton;

    private SkillUpgradeItemViewModel _viewModel;
    private bool _hasAllReferences;

    private void Awake()
    {
        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_icon, nameof(_icon), this);
        hasAllReferences &= UnityUtility.CheckReference(_nameText, nameof(_nameText), this);
        hasAllReferences &= UnityUtility.CheckReference(_cooldown, nameof(_cooldown), this);
        hasAllReferences &= UnityUtility.CheckReference(_levelText, nameof(_levelText), this);
        hasAllReferences &= UnityUtility.CheckReference(_dpsText, nameof(_dpsText), this);
        hasAllReferences &= UnityUtility.CheckReference(_costText, nameof(_costText), this);
        hasAllReferences &= UnityUtility.CheckReference(_progressFill, nameof(_progressFill), this);
        hasAllReferences &= UnityUtility.CheckReference(_upgradeButton, nameof(_upgradeButton), this);

        _hasAllReferences = hasAllReferences;
        enabled = hasAllReferences;

        if (!hasAllReferences) { return; }

        _upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    private void OnDestroy()
    {
        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
        }

        ReleaseViewModel();
    }

    private void Update()
    {
        RefreshCooldown();
    }

    public void Bind(string skillId)
    {
        if (!_hasAllReferences) { return; }

        if (_viewModel == null || !string.Equals(_viewModel.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
        {
            ReleaseViewModel();
            _viewModel = new SkillUpgradeItemViewModel(skillId);
            _viewModel.PropertyChanged += OnPropertyChanged;
        }

        RefreshAll();
    }

    private void ReleaseViewModel()
    {
        if (_viewModel == null) { return; }

        _viewModel.PropertyChanged -= OnPropertyChanged;
        _viewModel.Dispose();
        _viewModel = null;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SkillUpgradeItemViewModel.Level):
                RefreshLevel();
                break;
            case nameof(SkillUpgradeItemViewModel.CanUpgrade):
                RefreshCanUpgrade();
                break;
            case nameof(SkillUpgradeItemViewModel.UseProgress):
                RefreshProgress();
                break;
            case nameof(SkillUpgradeItemViewModel.DpsText):
                RefreshDps();
                break;
        }
    }

    private void OnUpgradeClicked()
    {
        _viewModel?.Upgrade();
    }

    private void RefreshAll()
    {
        Sprite icon = _viewModel.Icon;

        _icon.sprite = icon;
        _icon.enabled = icon != null;

        _nameText.text = _viewModel.Name;
        RefreshLevel();
        RefreshProgress();
        RefreshDps();
        RefreshCanUpgrade();
        RefreshCooldown();
    }

    private void RefreshLevel()
    {
        _levelText.text = _viewModel.LevelText;
        _costText.text = _viewModel.CostText;
    }

    private void RefreshDps()
    {
        _dpsText.text = _viewModel.DpsText;
    }

    private void RefreshProgress()
    {
        _progressFill.fillAmount = _viewModel.UseProgress;
    }

    private void RefreshCanUpgrade()
    {
        _upgradeButton.interactable = _viewModel.CanUpgrade;
    }

    private void RefreshCooldown()
    {
        if (_viewModel == null) { return; }

        _cooldown.fillAmount = _viewModel.CooldownRatio;
    }
}
