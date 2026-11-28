using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class HudView : UIView
{
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _blockCountText;
    [SerializeField] private MinimapView _minimapView;

    [SerializeField] private SkillUpgradeItemView _upgradeItemPrefab;
    [SerializeField] private Transform _upgradeItemRoot;

    private readonly List<SkillUpgradeItemView> _upgradeItems = new List<SkillUpgradeItemView>();

    private HudViewModel _viewModel;
    private bool _isBound;

    private void OnDestroy()
    {
        Unbind();

        if (_viewModel != null)
        {
            _viewModel.Dispose();
            _viewModel = null;
        }
    }

    public void SetPlayer(Transform player)
    {
        if (_minimapView == null) { return; }

        _minimapView.Bind(player);
    }

    protected override bool OnCreate()
    {
        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_goldText, nameof(_goldText), this);
        hasAllReferences &= UnityUtility.CheckReference(_stageText, nameof(_stageText), this);
        hasAllReferences &= UnityUtility.CheckReference(_blockCountText, nameof(_blockCountText), this);
        hasAllReferences &= UnityUtility.CheckReference(_minimapView, nameof(_minimapView), this);
        hasAllReferences &= UnityUtility.CheckReference(_upgradeItemPrefab, nameof(_upgradeItemPrefab), this);
        hasAllReferences &= UnityUtility.CheckReference(_upgradeItemRoot, nameof(_upgradeItemRoot), this);

        if (!hasAllReferences) { return false; }

        _viewModel = new HudViewModel();

        return true;
    }

    protected override void OnShow()
    {
        Bind();
        RefreshAll();
    }

    protected override void OnHide()
    {
        Unbind();
    }

    private void Bind()
    {
        if (_isBound || _viewModel == null) { return; }

        _viewModel.PropertyChanged += OnPropertyChanged;
        _isBound = true;
    }

    private void Unbind()
    {
        if (!_isBound) { return; }

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged;
        }

        _isBound = false;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(HudViewModel.GoldText):
                RefreshGold();
                break;
            case nameof(HudViewModel.StageText):
                RefreshStage();
                break;
            case nameof(HudViewModel.BlockCountText):
                RefreshBlockCount();
                break;
            case nameof(HudViewModel.SkillIds):
                RefreshUpgradeItems();
                break;
        }
    }

    private void RefreshAll()
    {
        if (_viewModel == null) { return; }

        RefreshGold();
        RefreshStage();
        RefreshBlockCount();
        RefreshUpgradeItems();
    }

    private void RefreshGold()
    {
        _goldText.text = _viewModel.GoldText;
    }

    private void RefreshStage()
    {
        _stageText.text = _viewModel.StageText;
    }

    private void RefreshBlockCount()
    {
        _blockCountText.text = _viewModel.BlockCountText;
    }

    private void RefreshUpgradeItems()
    {
        if (_viewModel == null) { return; }

        IReadOnlyList<string> skillIds = _viewModel.SkillIds;

        MatchUpgradeItemCount(skillIds.Count);

        for (int i = 0; i < skillIds.Count; i++)
        {
            SkillUpgradeItemView item = _upgradeItems[i];

            item.transform.SetSiblingIndex(i);
            item.Bind(skillIds[i]);
        }
    }

    private void MatchUpgradeItemCount(int count)
    {
        while (_upgradeItems.Count > count)
        {
            int last = _upgradeItems.Count - 1;
            GameObject removed = _upgradeItems[last].gameObject;

            _upgradeItems.RemoveAt(last);
            removed.SetActive(false);
            Destroy(removed);
        }

        while (_upgradeItems.Count < count)
        {
            _upgradeItems.Add(Instantiate(_upgradeItemPrefab, _upgradeItemRoot, false));
        }
    }
}
