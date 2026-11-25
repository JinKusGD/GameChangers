using System;
using System.ComponentModel;

public class PlayerViewModel : INotifyPropertyChanged, IDisposable
{
    private PlayerModel _model;

    public event PropertyChangedEventHandler PropertyChanged;

    public float MoveSpeed => _model.MoveSpeed;
    public double AttackPower => _model.AttackPower;

    public PlayerViewModel()
    {
        _model = ModelManager.Instance.PlayerModel;
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

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_model.MoveSpeed):
            case nameof(_model.AttackPower):
                PropertyChanged?.Invoke(this, e);
                break;
        }
    }


}
