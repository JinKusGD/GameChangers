public class StateMachine<TState> where TState : class, IState
{
    public TState CurrentState { get; private set; }

    public void ChangeState(TState nextState)
    {
        if (nextState == null) { return; }

        CurrentState?.OnExit();
        CurrentState = nextState;
        CurrentState.OnEnter();
    }

    public void OnUpdate()
    {
        CurrentState?.OnUpdate();
    }
}
