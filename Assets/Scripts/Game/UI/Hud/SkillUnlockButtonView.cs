using System.ComponentModel;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUnlockButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _costText;

    private SkillUnlockButtonViewModel _viewModel;

    private void Awake()
    {
        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_button, nameof(_button), this);
        hasAllReferences &= UnityUtility.CheckReference(_costText, nameof(_costText), this);

        if (!hasAllReferences)
        {
            enabled = false;
            return;
        }

        _viewModel = new SkillUnlockButtonViewModel();
        _viewModel.PropertyChanged += OnPropertyChanged;
        _button.onClick.AddListener(OnClicked);

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged;
            _viewModel.Dispose();
            _viewModel = null;
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SkillUnlockButtonViewModel.IsVisible):
                RefreshVisible();
                break;
            case nameof(SkillUnlockButtonViewModel.CostText):
                RefreshCost();
                break;
            case nameof(SkillUnlockButtonViewModel.CanUnlock):
                RefreshInteractable();
                break;
        }
    }

    private void RefreshAll()
    {
        RefreshCost();
        RefreshInteractable();
        RefreshVisible();
    }

    private void RefreshCost()
    {
        _costText.text = _viewModel.CostText;
    }

    private void RefreshInteractable()
    {
        _button.interactable = _viewModel.CanUnlock;
    }

    private void RefreshVisible()
    {
        gameObject.SetActive(_viewModel.IsVisible);
    }

    private void OnClicked()
    {
        ShowPopupAsync().Forget();
    }

    private static async UniTaskVoid ShowPopupAsync()
    {
        UIManager uiManager = UIManager.Instance;

        if (uiManager == null) { return; }

        await uiManager.ShowAsync<SkillUnlockPopup>();
    }
}
