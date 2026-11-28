using System;
using System.ComponentModel;

public class SkillUnlockButtonViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly PropertyChangedEventArgs IsVisibleChangedArgs = new PropertyChangedEventArgs(nameof(IsVisible));
    private static readonly PropertyChangedEventArgs CanUnlockChangedArgs = new PropertyChangedEventArgs(nameof(CanUnlock));
    private static readonly PropertyChangedEventArgs CostTextChangedArgs = new PropertyChangedEventArgs(nameof(CostText));

    private SkillModel _skillModel;
    private CurrencyModel _currencyModel;

    private bool _canUnlock;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsVisible
    {
        get { return SkillUnlockService.HasEmptySlot; }
    }

    public bool CanUnlock
    {
        get { return _canUnlock; }
    }

    public string CostText
    {
        get { return NumberUtility.Format(SkillUnlockService.Cost); }
    }

    public SkillUnlockButtonViewModel()
    {
        _skillModel = ModelManager.Instance.SkillModel;
        _currencyModel = ModelManager.Instance.CurrencyModel;

        _skillModel.PropertyChanged += OnSkillModelPropertyChanged;
        _currencyModel.PropertyChanged += OnCurrencyModelPropertyChanged;

        _canUnlock = SkillUnlockService.CanUnlock;
    }

    public void Dispose()
    {
        if (_skillModel != null)
        {
            _skillModel.PropertyChanged -= OnSkillModelPropertyChanged;
            _skillModel = null;
        }

        if (_currencyModel != null)
        {
            _currencyModel.PropertyChanged -= OnCurrencyModelPropertyChanged;
            _currencyModel = null;
        }

        PropertyChanged = null;
    }

    private void OnSkillModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SkillModel.OwnedSkillIds):
            case nameof(SkillModel.MaxOwnedCount):
                PropertyChanged?.Invoke(this, IsVisibleChangedArgs);
                PropertyChanged?.Invoke(this, CostTextChangedArgs);
                UpdateCanUnlock();
                break;
        }
    }

    private void OnCurrencyModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrencyModel.Gold))
        {
            UpdateCanUnlock();
        }
    }

    private void UpdateCanUnlock()
    {
        bool canUnlock = SkillUnlockService.CanUnlock;

        if (_canUnlock == canUnlock) { return; }

        _canUnlock = canUnlock;
        PropertyChanged?.Invoke(this, CanUnlockChangedArgs);
    }
}
