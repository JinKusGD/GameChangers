using System.ComponentModel;

public class PlayerModel : ObservableModel
{
    private static readonly PropertyChangedEventArgs MoveSpeedChangedArgs = new PropertyChangedEventArgs(nameof(MoveSpeed));
    private static readonly PropertyChangedEventArgs AttackPowerChangedArgs = new PropertyChangedEventArgs(nameof(AttackPower));

    private float _moveSpeed;
    private double _attackPower;

    public float MoveSpeed
    {
        get => _moveSpeed;
        set => SetProperty(ref _moveSpeed, value, MoveSpeedChangedArgs);
    }

    public double AttackPower
    {
        get => _attackPower;
        set => SetProperty(ref _attackPower, value, AttackPowerChangedArgs);
    }

    public PlayerModel(float moveSpeed, double attackPower)
    {
        _moveSpeed = moveSpeed;
        _attackPower = attackPower;
    }
}
