public class PlayerAttackState : PlayerState
{
    private bool _hasEnteredAttackAnimation;

    public float AttackInterval { get; set; }

    public PlayerAttackState(PlayerContext context) : base(context) { }

    public override void OnEnter()
    {
        _hasEnteredAttackAnimation = false;
        Animation.FitAttackSpeedToInterval(AttackInterval);
        Animation.Play(PlayerAnimationHash.Attack);
    }

    public override void OnUpdate()
    {
        Movement.FaceTarget();

        bool isInAttackAnimation = Animation.IsInOrEntering(PlayerAnimationHash.Attack);

        if (!_hasEnteredAttackAnimation)
        {
            _hasEnteredAttackAnimation = isInAttackAnimation;
            return;
        }

        if (!isInAttackAnimation)
        {
            ChangeState(Context.IdleState);
        }
    }

    public override void OnAttackRequested(float attackInterval)
    {
        AttackInterval = attackInterval;
        ChangeState(this);
    }
}
