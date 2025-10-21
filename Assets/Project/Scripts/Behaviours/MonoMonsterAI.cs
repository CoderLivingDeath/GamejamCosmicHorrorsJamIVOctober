using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Stateless;
using UnityEngine;

public class MonoMonsterAI : MonoCharacter
{
    private MonsterAIStateMachine monsterAI;

    [SerializeField]
    private MonoCharacterController monoCharacterController;

    [SerializeField]
    private MonsterVisionTrigger AIVision;

    [SerializeField]
    private MonsterAIStateMachine.AIState aIState;

    #region Corutines
    private UniTaskCoroutine MoveToPointCorutine;

    private UniTaskCoroutine<int> DelayCorutine;

    private UniTaskCoroutine IdleLoopCorutine;

    public async UniTask MoveToPoint(CancellationToken token = default)
    {
        //TODO: переработать систему пути
        Vector3 Point = transform.position + new Vector3(UnityEngine.Random.Range(-10, 10), 0, 0);
        float StopDistance = 1f;

        while (!token.IsCancellationRequested)
        {
            if (Vector3.Distance(Point, transform.position) < StopDistance)
            {
                break;
            }

            Vector3 direction = (Point - transform.position).normalized;
            monoCharacterController.MoveToDirection(new Vector2(direction.x, direction.z));

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        StopMovement();
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdle);
    }

    private async UniTask DelayTask(int milliseconds, CancellationToken token = default)
    {
        await UniTask.Delay(milliseconds, cancellationToken: token);
    }

    private async UniTask IdleLoopTask(CancellationToken token = default)
    {
        //TODO: Перерабоать поведение
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(500, cancellationToken: token);
            int choice = UnityEngine.Random.Range(0, 3);
            switch (choice)
            {
                case 0:
                    monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdleWalk);
                    //Debug.Log("Transition to IdleWalk");
                    break;
                case 1:
                    monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdleStandTall);
                    //Debug.Log("Transition to IdleStandTall");
                    break;
                case 2:
                    await UniTask.Delay(UnityEngine.Random.Range(0, 10000), cancellationToken: token);
                    // Debug.Log("Remain in Idle state");
                    break;
            }
        }
    }

    #endregion

    #region Unity Internal

    [Space]
    [Header("State variables")]
    public Transform Target;
    public float SerachDuration = 10f;
    public float SearchTime;

    void Awake()
    {
        AIVision.TargetEntered += TargetSpoted;
        AIVision.TargetExited += TargetLost;
        ConfigureStates();

        MoveToPointCorutine = new(MoveToPoint);
        DelayCorutine = new(DelayTask);
        IdleLoopCorutine = new(IdleLoopTask);
    }

    void Start()
    {
        monsterAI.EnterState(monsterAI.CurrentState);
    }

    private void Update()
    {
        aIState = monsterAI.CurrentState;

        InternalUpdate();
    }

    public void InternalUpdate()
    {
        switch (aIState)
        {
            case MonsterAIStateMachine.AIState.Idle:
                UpdateIdle();
                break;

            case MonsterAIStateMachine.AIState.IdleWalk:
                UpdateIdleWalk();
                break;

            case MonsterAIStateMachine.AIState.IdleStandTall:
                UpdateIdleStandTall();
                break;

            case MonsterAIStateMachine.AIState.Search:
                UpdateSearch();
                break;

            case MonsterAIStateMachine.AIState.Chase:
                UpdateChase();
                break;

            case MonsterAIStateMachine.AIState.Agressive:
                UpdateAgressive();
                break;
        }
    }

    void OnDestroy()
    {
        MoveToPointCorutine?.Dispose();
        DelayCorutine?.Dispose();
        IdleLoopCorutine?.Dispose();
    }
    #endregion

    private async void UpdateIdleWalk()
    {
        //Debug.Log("Updating IdleWalk state");
    }

    // Обновление для IdleWalk
    private void UpdateIdleStandTall()
    {
        //Debug.Log("Updating StandTall state");
        // Логика обновления для легкой ходьбы бездействия
    }

    private void UpdateIdle()
    {
        //Debug.Log("Updating Idle state");
        // Логика Idle
    }
    private void UpdateChase()
    {
        // Debug.Log("Updating Chase state");

        var direction = Target.position - transform.position;
        monoCharacterController.MoveToDirection(direction.normalized);
    }

    private void UpdateAgressive()
    {
        // Debug.Log("Updating Agressive state");
        // Логика Agressive
    }

    private float RotateInterval = 1.5f; // Период поворота в секундах
    private float RotateTimer = 0f;
    private bool RotateRightNext = true;

    private void UpdateSearch()
    {
        // Debug.Log("Updating Search state");

        SearchTime += Time.deltaTime;
        RotateTimer += Time.deltaTime;

        if (RotateTimer >= RotateInterval)
        {
            if (RotateRightNext)
                monoCharacterController.RotateRight();
            else
                monoCharacterController.RotateLeft();

            RotateRightNext = !RotateRightNext;
            RotateTimer = 0f;
        }

        if (SearchTime >= SerachDuration)
        {
            SearchTimeout();
            SearchTime = 0f;
        }
    }

    private void SearchTimeout()
    {
        // Debug.Log("Search timed out");
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.SearchTimeout);
    }

    private void TargetSpoted(Transform target)
    {
        // Debug.Log($"TargetSpoted: {target.name}");
        Target = target;
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.TargetSpotted);
    }

    private void TargetLost(Transform target)
    {
        // Debug.Log($"TargetLost: {target.name}");
        Target = null;
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.LostTarget);
    }

    private void StopMovement()
    {
        monoCharacterController.MoveToDirection(Vector3.zero);
    }

    private void ConfigureStates()
    {
        monsterAI = new();
        // Обработчики для Idle
        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Idle,
            onEnter: () =>
            {
                // Debug.Log("Entered Idle state");

                IdleLoopCorutine.Run();
            },
            onExit: () =>
            {
                // Debug.Log("Exited Idle state");
                // Дополнительная логика при выходе из Idle

                IdleLoopCorutine.Stop();
            }
        );


        // Обработчики для IdleWalk
        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.IdleWalk,
            onEnter: () =>
            {
                // Debug.Log("Entered IdleWalk state");
                // Логика при входе в IdleWalk\

                if (MoveToPointCorutine.IsRunning)
                    MoveToPointCorutine.Stop();

                MoveToPointCorutine.Run();
            },
            onExit: () =>
            {
                // Debug.Log("Exited IdleWalk state");
                // Логика при выходе из IdleWalk
                MoveToPointCorutine.Stop();
            }
        );

        // Обработчики для IdleStandTall
        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.IdleStandTall,
            onEnter: () =>
            {
                // Debug.Log("Entered IdleStandTall state");
                // Дополнительная логика при входе в IdleStandTall

                DelayCorutine.RunAsync(5000).ContinueWith(() => monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdle)).Forget();
            },
            onExit: () =>
            {
                // Debug.Log("Exited IdleStandTall state");
                // Дополнительная логика при выходе из IdleStandTall
                DelayCorutine.Stop();
            }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Search,
            onEnter: () =>
            {
                // Логика при входе в Search
                // Debug.Log("Entered Search state");
            },
            onExit: () =>
            {
                // Логика при выходе из Search
                // Debug.Log("Exited Search state");

                SearchTime = 0f;
            }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Chase,
            onEnter: () =>
            {
                // Логика при входе в Chase
                // Debug.Log("Entered Chase state");
            },
            onExit: () =>
            {
                StopMovement();
                // Debug.Log("Exited Chase state");
            }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Agressive,
            onEnter: () =>
            {
                // Логика при входе в Agressive
                // Debug.Log("Entered Agressive state");
            },
            onExit: () =>
            {
                // Логика при выходе из Agressive
                // Debug.Log("Exited Agressive state");
            }
        );
    }
}

public class MonsterAIStateMachine
{
    public enum AIState
    {
        Idle,
        IdleStandTall, // TODO: для этого состояния нужны анимации
        IdleWalk,
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
    }

    private readonly StateMachine<AIState, AITrigger> _stateMachine;
    private readonly Dictionary<AIState, StateHandler<AIState>> _handlers = new();
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
            .Permit(AITrigger.Agressive, AIState.Agressive);

        _stateMachine.Configure(AIState.IdleWalk)
        .SubstateOf(AIState.Idle)
        .Permit(AITrigger.GoIdle, AIState.Idle);

        _stateMachine.Configure(AIState.IdleStandTall)
        .SubstateOf(AIState.Idle)
        .Permit(AITrigger.GoIdle, AIState.Idle);

        _stateMachine
            .Configure(AIState.Search)
            .Permit(AITrigger.SearchTimeout, AIState.Idle)
            .Permit(AITrigger.TargetSpotted, AIState.Chase)
            .Permit(AITrigger.LostTarget, AIState.Idle)
            .Permit(AITrigger.Agressive, AIState.Agressive);

        _stateMachine.Configure(AIState.Agressive).Permit(AITrigger.LostTarget, AIState.Search);

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

    public AIState CurrentState => _stateMachine.State;
}
