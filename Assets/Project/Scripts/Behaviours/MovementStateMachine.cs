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
            .Permit(Trigger.Run, State.Run)
            .OnEntry(() => OnStateChanged?.Invoke(State.Idle));

        _stateMachine.Configure(State.Move)
            .Permit(Trigger.Idle, State.Idle)
            .Permit(Trigger.Run, State.Run)
            .OnEntry(() => OnStateChanged?.Invoke(State.Move));

        _stateMachine.Configure(State.Run)
            .Permit(Trigger.Idle, State.Idle)
            .Permit(Trigger.Move, State.Move)
            .OnEntry(() => OnStateChanged?.Invoke(State.Run));
    }

    private void FireTrigger(Trigger trigger)
    {
        if (_stateMachine.CanFire(trigger))
        {
            _stateMachine.Fire(trigger);
        }
    }

    public void UpdateState(bool isMove, bool isRun = false)
    {
        if (isRun)
        {
            FireTrigger(Trigger.Run);
        }
        else if (isMove)
        {
            FireTrigger(Trigger.Move);
        }
        else
        {
            FireTrigger(Trigger.Idle);
        }
    }
}
