using System;
using System.Collections.Generic;
using Stateless;

public class MonsterAIStateMachine
{
    public enum AIState
    {
        Idle,
        IdleStandTall, // TODO: для этого состояния нужны анимации
        IdleWalk,
        Move,
        Search,
        Chase,
        Agressive, // TODO: нужно состояние атаки
    }

    public enum AITrigger
    {
        StartSearching,
        TargetSpotted,
        LostTarget,
        SearchTimeout,
        Agressive,
        GoIdleWalk,
        GoIdleStandTall,
        GoIdle,
        GoMove
    }

    private readonly StateMachine<AIState, AITrigger> _stateMachine;
    private readonly Dictionary<AIState, StateHandler<AIState>> _handlers = new();

    public AIState CurrentState => _stateMachine.State;
    public event Action<AIState> StateChanged;

    public MonsterAIStateMachine()
    {
        _stateMachine = new StateMachine<AIState, AITrigger>(AIState.Idle);

        _stateMachine.OnTransitioned(t =>
        {
            ExitState(t.Source);
            EnterState(t.Destination);
            StateChanged?.Invoke(t.Destination);
        });

        _stateMachine
            .Configure(AIState.Idle)
            .Permit(AITrigger.GoIdleWalk, AIState.IdleWalk)
            .Permit(AITrigger.GoIdleStandTall, AIState.IdleStandTall)
            .Permit(AITrigger.StartSearching, AIState.Search)
            .Permit(AITrigger.TargetSpotted, AIState.Chase)
            .Permit(AITrigger.Agressive, AIState.Agressive)
            .Permit(AITrigger.GoMove, AIState.Move);          // Добавляем триггер перехода в Move

        _stateMachine.Configure(AIState.IdleWalk)
            .SubstateOf(AIState.Idle)
            .Permit(AITrigger.GoIdle, AIState.Idle);

        _stateMachine.Configure(AIState.IdleStandTall)
            .SubstateOf(AIState.Idle)
            .Permit(AITrigger.GoIdle, AIState.Idle);

        _stateMachine.Configure(AIState.Move)
            .SubstateOf(AIState.Idle)                         // Можно сделать подстатусом Idle, если нужно
            .Permit(AITrigger.GoIdle, AIState.Idle)
            .Permit(AITrigger.TargetSpotted, AIState.Chase)
            .Permit(AITrigger.LostTarget, AIState.Idle);

        _stateMachine
            .Configure(AIState.Search)
            .Permit(AITrigger.SearchTimeout, AIState.Idle)
            .Permit(AITrigger.TargetSpotted, AIState.Chase)
            .Permit(AITrigger.LostTarget, AIState.Idle)
            .Permit(AITrigger.Agressive, AIState.Agressive);

        _stateMachine.Configure(AIState.Agressive)
            .Permit(AITrigger.LostTarget, AIState.Search);

        _stateMachine
            .Configure(AIState.Chase)
            .SubstateOf(AIState.Agressive)
            .Permit(AITrigger.LostTarget, AIState.Search)
            .Permit(AITrigger.Agressive, AIState.Agressive);
    }


    public void Subscribe(AIState state, Action onEnter, Action onExit)
    {
        if (!_handlers.ContainsKey(state))
        {
            _handlers[state] = new StateHandler<AIState>();
        }
        _handlers[state].OnEnter = onEnter;
        _handlers[state].OnExit = onExit;
    }

    public void EnterState(AIState state)
    {
        if (_handlers.TryGetValue(state, out var handler))
        {
            handler.Enter();
        }
    }

    public void ExitState(AIState state)
    {
        if (_handlers.TryGetValue(state, out var handler))
        {
            handler.Exit();
        }
    }

    public bool IsInState(AIState state)
    {
        return _stateMachine.IsInState(state);
    }

    private readonly object _tryFireLock = new object();

    public bool TryFire(AITrigger trigger)
    {
        lock (_tryFireLock)
        {
            if (_stateMachine.CanFire(trigger))
            {
                _stateMachine.Fire(trigger);
                return true;
            }
            return false;
        }
    }
}
