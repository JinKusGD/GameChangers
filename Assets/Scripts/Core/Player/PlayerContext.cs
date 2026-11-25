public class PlayerContext
{
    public PlayerMovement Movement { get; }
    public PlayerAnimation Animation { get; }
    public StateMachine<PlayerState> StateMachine { get; }

    public PlayerIdleState IdleState { get; }
    public PlayerMoveState MoveState { get; }
    public PlayerAttackState AttackState { get; }

    public PlayerContext(PlayerMovement movement, PlayerAnimation animation)
    {
        Movement = movement;
        Animation = animation;
        StateMachine = new StateMachine<PlayerState>();

        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        AttackState = new PlayerAttackState(this);
    }
}
