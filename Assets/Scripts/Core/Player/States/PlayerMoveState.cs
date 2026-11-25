public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerContext context) : base(context) { }

    public override void OnEnter()
    {
        Animation.CrossFade(PlayerAnimationHash.Run);
    }

    public override void OnUpdate()
    {
        if (!Movement.HasTarget || Movement.HasArrived)
        {
            ChangeState(Context.IdleState);
            return;
        }

        Movement.FaceTarget();
        Movement.MoveTowardTarget();
    }

    public override void OnAttackRequested()
    {
        TryEnterAttack();
    }
}
