using System;
using System.Collections.Generic;
using System.ComponentModel;

public class HudViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly PropertyChangedEventArgs GoldTextChangedArgs = new PropertyChangedEventArgs(nameof(GoldText));
    private static readonly PropertyChangedEventArgs StageTextChangedArgs = new PropertyChangedEventArgs(nameof(StageText));
    private static readonly PropertyChangedEventArgs BlockCountTextChangedArgs = new PropertyChangedEventArgs(nameof(BlockCountText));
    private static readonly PropertyChangedEventArgs SkillIdsChangedArgs = new PropertyChangedEventArgs(nameof(SkillIds));

    private CurrencyModel _currencyModel;
    private StageModel _stageModel;
    private SkillModel _skillModel;

    public event PropertyChangedEventHandler PropertyChanged;

    public string GoldText => NumberUtility.Format(Math.Floor(_currencyModel.Gold));
    public string StageText => $"{_stageModel.Chapter}-{_stageModel.Stage}";
    public string BlockCountText => _stageModel.AliveBlockCount.ToString();

    public IReadOnlyList<string> SkillIds => _skillModel.OwnedSkillIds;

    public HudViewModel()
    {
        _currencyModel = ModelManager.Instance.CurrencyModel;
        _stageModel = ModelManager.Instance.StageModel;

        _currencyModel.PropertyChanged += OnCurrencyModelPropertyChanged;
        _stageModel.PropertyChanged += OnStageModelPropertyChanged;

        _skillModel = ModelManager.Instance.SkillModel;
        _skillModel.PropertyChanged += OnSkillModelPropertyChanged;
    }

    public void Dispose()
    {
        if (_currencyModel != null)
        {
            _currencyModel.PropertyChanged -= OnCurrencyModelPropertyChanged;
            _currencyModel = null;
        }

        if (_stageModel != null)
        {
            _stageModel.PropertyChanged -= OnStageModelPropertyChanged;
            _stageModel = null;
        }

        if (_skillModel != null)
        {
            _skillModel.PropertyChanged -= OnSkillModelPropertyChanged;
            _skillModel = null;
        }
    }

    private void OnCurrencyModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CurrencyModel.Gold):
                PropertyChanged?.Invoke(this, GoldTextChangedArgs);
                break;
        }
    }

    private void OnStageModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(StageModel.Chapter):
            case nameof(StageModel.Stage):
                PropertyChanged?.Invoke(this, StageTextChangedArgs);
                break;
            case nameof(StageModel.AliveBlockCount):
                PropertyChanged?.Invoke(this, BlockCountTextChangedArgs);
                break;
        }
    }

    private void OnSkillModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SkillModel.OwnedSkillIds):
                PropertyChanged?.Invoke(this, SkillIdsChangedArgs);
                break;
        }
    }
}
