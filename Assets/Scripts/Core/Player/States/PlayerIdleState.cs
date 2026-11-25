public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerContext context) : base(context) { }

    public override void OnEnter()
    {
        Animation.CrossFade(PlayerAnimationHash.Idle);
    }

    public override void OnUpdate()
    {
        Movement.FaceTarget();

        if (Movement.HasTarget && !Movement.HasArrived)
        {
            ChangeState(Context.MoveState);
        }
    }

    public override void OnAttackRequested()
    {
        TryEnterAttack();
    }
}
