using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
// ---------------------------------------
public class InteractionController : IDisposable
{
    public Transform Transform;

    private float _radius;
    public IEnumerable<MonoInteractable> Interactables;
    private MonoInteractable _selectedInteractable;
    public MonoInteractable SelectedInteractable
    {
        get => _selectedInteractable;
        private set
        {
            if (_selectedInteractable != value)
            {
                _selectedInteractable = value;
                SelectedInteractableChanged?.Invoke(_selectedInteractable);
            }
        }
    }

    public event Action<MonoInteractable> SelectedInteractableChanged;


    private LayerMask _mask;
    private PlayerLoopTiming _timing = PlayerLoopTiming.Update;

    public float Radius { get => _radius; set => _radius = value; }
    public LayerMask Mask { get => _mask; set => _mask = value; }
    public PlayerLoopTiming Timing { get => _timing; set => _timing = value; }

    // private UniTaskCoroutine _monitoringCoroutine;
    // public UniTaskCoroutine MonitoringCoroutine => _monitoringCoroutine;

    public InteractionController(Transform transform)
    {
        Transform = transform;
        // // Создаём корутину мониторинга в конструкторе, но пока не запускаем
        // _monitoringCoroutine = new UniTaskCoroutine(ct => InteractableMonitoring(ct));
    }

    // private async UniTask InteractableMonitoring(CancellationToken cancellationToken)
    // {
    //     while (!cancellationToken.IsCancellationRequested)
    //     {
    //         Interactables = GetInteractables(Transform.position);
    //         await UniTask.Yield(_timing, cancellationToken);
    //     }
    //     Interactables = null;
    // }

    public IEnumerable<MonoInteractable> FindInteractables(Vector3 origin, float radius, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius, mask);
        HashSet<MonoInteractable> result = new HashSet<MonoInteractable>();
        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<MonoInteractable>();
            if (interactable != null)
            {
                result.Add(interactable);
            }
        }
        return result;
    }

    public IEnumerable<MonoInteractable> GetInteractables(Vector3 position)
    {
        return FindInteractables(position, _radius, _mask)
            .OrderBy(item => Vector3.Distance(position, item.transform.position));
    }

    public IEnumerable<MonoInteractable> GetInteractables(Vector3 position,
            Func<MonoInteractable, bool> additionalFilter = null,
            Func<MonoInteractable, float> orderBySelector = null,
            LayerMask obstacleLayerMask = default)
    {
        List<MonoInteractable> interactables = new List<MonoInteractable>();

        Collider[] hits = Physics.OverlapSphere(position, _radius, _mask);
        for (int i = 0; i < hits.Length; i++)
        {
            var interactable = hits[i].GetComponent<MonoInteractable>();
            if (interactable == null)
                continue;

            if (!interactable.CanInteract() || !interactable.IsWithinInteractionBounds(position))
                continue;

            if (additionalFilter != null && !additionalFilter(interactable))
                continue;

            Vector3 direction = (hits[i].transform.position - position).normalized;
            float distance = Vector3.Distance(position, hits[i].transform.position);

            if (Physics.Raycast(position, direction, out RaycastHit hitInfo, distance, obstacleLayerMask))
            {
                if (hitInfo.collider != hits[i])
                    continue;
            }

            interactables.Add(interactable);
        }

        if (orderBySelector != null)
        {
            interactables.Sort((a, b) =>
                Comparer<float>.Default.Compare(orderBySelector(a), orderBySelector(b)));
        }
        else
        {
            interactables.Sort((a, b) =>
            {
                int priorityComparison = b.Priority.CompareTo(a.Priority);
                if (priorityComparison != 0) return priorityComparison;

                float distA = (a.transform.position - position).sqrMagnitude;
                float distB = (b.transform.position - position).sqrMagnitude;
                return distA.CompareTo(distB);
            });
        }

        return interactables;
    }

    public void UpdateInteractables(Vector3 position,
        Func<MonoInteractable, bool> additionalFilter = null,
        Func<MonoInteractable, float> orderBySelector = null,
         LayerMask obstacleLayerMask = default)
    {
        Interactables = GetInteractables(position, additionalFilter, orderBySelector, obstacleLayerMask);
        
        // Обновляем выбранный интерактивный объект (первый из списка или null)
        SelectedInteractable = Interactables.FirstOrDefault();
    }
    public void Interact()
    {
        
        Debug.Log(SelectedInteractable);
        if (SelectedInteractable != null)
        {
            SelectedInteractable.Interact(Transform.gameObject);
        }
    }

    public void InteractWith(MonoInteractable interactableBehaviour)
    {
        if (interactableBehaviour == null) return;
        interactableBehaviour.Interact(Transform.gameObject);
    }

    public void DrawGizmos(Vector3 position, Color defaultLineColor, float gizmoSize = 0.5f)
    {
        if (Interactables == null)
            return;

        Gizmos.color = new Color(defaultLineColor.r, defaultLineColor.g, defaultLineColor.b, 0.1f);
        Gizmos.DrawSphere(position, _radius); // Полупрозрачный радиус

        foreach (var interactable in Interactables)
        {
            if (interactable == null) continue;

            Vector3 objPos = interactable.transform.position;

            // Рисуем сферу у объекта
            Gizmos.color = defaultLineColor;
            Gizmos.DrawWireSphere(objPos, gizmoSize);
        }

        var first = SelectedInteractable;

        foreach (var interactable in Interactables)
        {
            if (interactable == null) continue;

            Vector3 objPos = interactable.transform.position;

            if (interactable == first)
            {
                Gizmos.color = Color.red; // Красная линия к выбранному объекту
            }
            else
            {
                Gizmos.color = defaultLineColor;
            }

            Gizmos.DrawLine(position, objPos);
        }
    }


    // Запуск мониторинга (если ещё не запущена)
    public void Enable()
    {
        // if (!_monitoringCoroutine.IsRunning)
        // {
        //     _monitoringCoroutine.Run();
        // }
    }

    // Остановка мониторинга
    public void Disable()
    {
        // if (_monitoringCoroutine.IsRunning)
        // {
        //     _monitoringCoroutine.Stop();
        // }
    }

    // Реализация IDisposable
    public void Dispose()
    {
        Disable();
        // _monitoringCoroutine?.Dispose();
        // _monitoringCoroutine = null;
    }
}