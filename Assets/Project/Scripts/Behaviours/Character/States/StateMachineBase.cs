using System;
using System.Collections.Generic;
using Stateless;

public class CharacterStateMachine
{
    public enum State
    {
        Idle,
        Walk,
        Run,
        Animating,
        Hiding
    }

    public enum Trigger
    {
        StartWalking,
        StartRunning,
        StopMoving,
        PlayAnimation,
        AnimationComplete,
        HidingStarted,
        HidingEnded
    }

    private readonly StateMachine<State, Trigger> _machine;
    private readonly Dictionary<State, StateHandler<State>> _handlers = new();

    public State CurrentState => _machine.State;

    public void Subscribe(State state, Action onEnter, Action onExit)
    {
        if (!_handlers.ContainsKey(state))
        {
            _handlers[state] = new StateHandler<State>();
        }
        _handlers[state].OnEnter = onEnter;
        _handlers[state].OnExit = onExit;
    }

    private void EnterState(State state)
    {
        if (_handlers.TryGetValue(state, out var handler))
        {
            handler.Enter();
        }
    }

    private void ExitState(State state)
    {
        if (_handlers.TryGetValue(state, out var handler))
        {
            handler.Exit();
        }
    }

    public CharacterStateMachine(State initialState)
    {
        _machine = new StateMachine<State, Trigger>(initialState);

        Configure();
        
        EnterState(initialState);
    }

    private void Configure()
    {
        _machine.OnTransitioned(t =>
        {
            ExitState(t.Source);
            EnterState(t.Destination);
        });

        _machine.Configure(State.Idle)
            .Permit(Trigger.StartWalking, State.Walk)
            .Permit(Trigger.StartRunning, State.Run)
            .Permit(Trigger.PlayAnimation, State.Animating)
            .Permit(Trigger.HidingStarted, State.Hiding);

        _machine.Configure(State.Walk)
            .Permit(Trigger.StopMoving, State.Idle)
            .Permit(Trigger.StartRunning, State.Run)
            .Permit(Trigger.PlayAnimation, State.Animating)
            .Permit(Trigger.HidingStarted, State.Hiding);

        _machine.Configure(State.Run)
            .Permit(Trigger.StopMoving, State.Idle)
            .Permit(Trigger.StartWalking, State.Walk)
            .Permit(Trigger.PlayAnimation, State.Animating)
            .Permit(Trigger.HidingStarted, State.Hiding);

        _machine.Configure(State.Animating)
            .Permit(Trigger.AnimationComplete, State.Idle)
            .Permit(Trigger.HidingStarted, State.Hiding);

        _machine.Configure(State.Hiding)
            .Permit(Trigger.HidingEnded, State.Idle)
            .Permit(Trigger.StartWalking, State.Walk)
            .Permit(Trigger.StartRunning, State.Run);

    }

    public bool TryFire(Trigger trigger)
    {
        if (_machine.CanFire(trigger))
        {
            _machine.Fire(trigger);
            return true;
        }
        return false;
    }
}

public class StateHandler<TState>
{
    public Action OnEnter { get; set; }
    public Action OnExit { get; set; }

    public void Enter() => OnEnter?.Invoke();
    public void Exit() => OnExit?.Invoke();
}
