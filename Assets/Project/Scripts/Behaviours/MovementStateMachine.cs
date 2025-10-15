using System;
using Stateless;

public class MovementStateMachine
{
    public enum State
    {
        Move,
        Run,
        Idle
    }

    public enum Trigger
    {
        Move,
        Run,
        Idle
    }

    public State CurrentState => _stateMachine.State;

    public event Action<State> OnStateChanged;

    private StateMachine<State, Trigger> _stateMachine;

    public MovementStateMachine()
    {
        ConfigureStateMachine();
    }

    private void ConfigureStateMachine()
    {
        _stateMachine = new StateMachine<State, Trigger>(State.Idle);

        _stateMachine.Configure(State.Idle)
        .Permit(Trigger.Move, State.Move)
        .OnEntry(() => OnStateChanged?.Invoke(State.Idle));

        _stateMachine.Configure(State.Move)
        .Permit(Trigger.Idle, State.Idle)
        .OnEntry(() => OnStateChanged?.Invoke(State.Move));

        // TODO: обработка run
    }

    private void FireTrigger(Trigger trigger)
    {
        if (_stateMachine.CanFire(trigger))
        {
            _stateMachine.Fire(trigger);
        }
    }

    public void UpdateState(bool isMove)
    {
        // TODO: обработка run
        if (isMove)
        {
            FireTrigger(Trigger.Move);
        }
        else
        {
            FireTrigger(Trigger.Idle);
        }
    }
}