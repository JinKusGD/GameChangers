using System.ComponentModel;

public class PlayerModel : INotifyPropertyChanged
{
    private readonly PropertyChangedEventArgs _moveSpeedPropertyChangedArgs = new PropertyChangedEventArgs(nameof(MoveSpeed));
    private readonly PropertyChangedEventArgs _attackPowerPropertyChangedArgs = new PropertyChangedEventArgs(nameof(AttackPower));

    private float _moveSpeed;
    private double _attackPower;

    public float MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            if (_moveSpeed == value)
            {
                return;
            }

            _moveSpeed = value;

            PropertyChanged?.Invoke(this, _moveSpeedPropertyChangedArgs);
        }
    }

    public double AttackPower
    {
        get => _attackPower;
        set
        {
            if (_attackPower == value)
            {
                return;
            }

            _attackPower = value;

            PropertyChanged?.Invoke(this, _attackPowerPropertyChangedArgs);
        }
    }

    public PlayerModel(float moveSpeed, double attackPower)
    {
        _moveSpeed = moveSpeed;
        _attackPower = attackPower;
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
