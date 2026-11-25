public abstract class PlayerState : IState
{
    protected readonly PlayerContext Context;

    protected PlayerMovement Movement => Context.Movement;
    protected PlayerAnimation Animation => Context.Animation;

    protected PlayerState(PlayerContext context)
    {
        Context = context;
    }

    public virtual void OnEnter() { }

    public abstract void OnUpdate();

    public virtual void OnExit() { }

    public virtual void OnAttackRequested() { }

    protected void ChangeState(PlayerState nextState)
    {
        Context.StateMachine.ChangeState(nextState);
    }

    protected void TryEnterAttack()
    {
        if (!Animation.HasAnimator) { return; }

        ChangeState(Context.AttackState);
    }
}
