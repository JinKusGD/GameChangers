using System.ComponentModel;
using TMPro;
using UnityEngine;

public class HudView : UIView
{
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _blockCountText;
    [SerializeField] private MinimapView _minimapView;

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

        _minimapView.SetPlayer(player);
    }

    protected override bool OnCreate()
    {
        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_goldText, nameof(_goldText), this);
        hasAllReferences &= UnityUtility.CheckReference(_stageText, nameof(_stageText), this);
        hasAllReferences &= UnityUtility.CheckReference(_blockCountText, nameof(_blockCountText), this);
        hasAllReferences &= UnityUtility.CheckReference(_minimapView, nameof(_minimapView), this);

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
        if (_isBound) { return; }

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
                _goldText.text = _viewModel.GoldText;
                break;
            case nameof(HudViewModel.StageText):
                _stageText.text = _viewModel.StageText;
                break;
            case nameof(HudViewModel.BlockCountText):
                _blockCountText.text = _viewModel.BlockCountText;
                break;
        }
    }

    private void RefreshAll()
    {
        _goldText.text = _viewModel.GoldText;
        _stageText.text = _viewModel.StageText;
        _blockCountText.text = _viewModel.BlockCountText;
    }
}
