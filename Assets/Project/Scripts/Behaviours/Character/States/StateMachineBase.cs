using System;
using System.Collections.Generic;
using Stateless;
using UnityEngine;

public class CharacterStateMachine
{
    public enum State
    {
        Idle,
        Active,
        Moving,
        Walking,
        Running,
        Hiding,
        Animation,
    }

    public enum Trigger
    {
        StartWalking,
        StartRunning,
        StopMoving,
        StartHiding,
        StopHiding,
        StartAnimation,
        StopAnimation,
        DoNothing,
    }

    private readonly StateMachine<State, Trigger> _machine;
    private readonly Dictionary<State, StateHandler<State>> _handlers = new();

    private Trigger? _triggerBuffer = null; // буфер триггеров во время анимации

    public State CurrentState => _machine.State;

    public event Action<State> StateChanged;

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

        _machine.OnTransitioned(t =>
        {
            ExitState(t.Source);
            EnterState(t.Destination);
            StateChanged?.Invoke(t.Destination);
        });

        Configure();

        EnterState(initialState);
        StateChanged?.Invoke(initialState);
    }

    private void Configure()
    {
        // Активное состояние персонажа
        _machine.Configure(State.Active);
        // .OnEntry(() => Debug.Log("Персонаж активен"))
        // .OnExit(() => Debug.Log("Персонаж неактивен"));

        // Состояние анимации (блокирует другие действия)
        _machine
            .Configure(State.Animation)
            .SubstateOf(State.Active)
            .Permit(Trigger.StopAnimation, State.Idle)
            .Ignore(Trigger.StartWalking)
            .Ignore(Trigger.StartRunning)
            .Ignore(Trigger.StartHiding)
            .Ignore(Trigger.StartAnimation)
            .OnEntry(() =>
            {
                // Debug.Log("Проигрывается анимация");
                _triggerBuffer = null; // Очистка буфера при входе в анимацию
            })
            .OnExit(() =>
            {
                // Debug.Log("Анимация завершена");
                // При выходе из анимации если буфер заполнен - воспроизводим сохраненный триггер
                if (_triggerBuffer.HasValue)
                {
                    var bufferedTrigger = _triggerBuffer.Value;
                    _triggerBuffer = null;
                    if (_machine.CanFire(bufferedTrigger))
                    {
                        _machine.Fire(bufferedTrigger);
                    }
                }
            });

        // Суперстейт движения
        _machine
            .Configure(State.Moving)
            .SubstateOf(State.Active)
            .Permit(Trigger.StopMoving, State.Idle)
            .Permit(Trigger.StartAnimation, State.Animation); // Переход в анимацию

        // Idle
        _machine
            .Configure(State.Idle)
            .Permit(Trigger.StartWalking, State.Walking)
            .Permit(Trigger.StartRunning, State.Running)
            .Permit(Trigger.StartHiding, State.Hiding)
            .Permit(Trigger.StartAnimation, State.Animation);

        // Walking под Moving
        _machine
            .Configure(State.Walking)
            .SubstateOf(State.Moving)
            .Permit(Trigger.StartRunning, State.Running)
            .Permit(Trigger.StopMoving, State.Idle)
            .Permit(Trigger.StartAnimation, State.Animation);

        // Running под Moving
        _machine
            .Configure(State.Running)
            .SubstateOf(State.Moving)
            .Permit(Trigger.StartWalking, State.Walking)
            .Permit(Trigger.StopMoving, State.Idle)
            .Permit(Trigger.StartAnimation, State.Animation);

        // Hiding под Active
        _machine
            .Configure(State.Hiding)
            .SubstateOf(State.Active)
            .Permit(Trigger.StopHiding, State.Idle)
            .Permit(Trigger.StartAnimation, State.Animation);
    }

    public bool IsInState(State superState)
    {
        return _machine.IsInState(superState);
    }

    public bool TryFire(Trigger trigger)
    {
        // Если в состоянии анимации и триггер не StopAnimation - буферизируем
        if (_machine.State == State.Animation && trigger != Trigger.StopAnimation)
        {
            _triggerBuffer = trigger;
            return false;
        }

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
