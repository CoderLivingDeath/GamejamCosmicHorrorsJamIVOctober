using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class InteractionBehaviour : MonoBehaviour
{
    public float Radius;
    public InteractableBehaviour SelectedInteractable => Interactables.FirstOrDefault();

    public IEnumerable<InteractableBehaviour> Interactables;

    public LayerMask Mask;

    public PlayerLoopTiming Timing => _timing;

    private UniTaskCoroutine _interactionMonitoringUniTaskCorutine;

    [SerializeField]
    private PlayerLoopTiming _timing;

    private async UniTask InteractableMonitoring(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Interactables = GetInteractables();
            await UniTask.Yield(_timing, cancellationToken);
        }
    }

    public IEnumerable<InteractableBehaviour> FindInteractables(Vector3 origin, float radius, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius, mask);
        HashSet<InteractableBehaviour> result = new HashSet<InteractableBehaviour>();
        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<InteractableBehaviour>();
            if (interactable != null)
            {
                result.Add(interactable);
            }
        }
        return result;
    }

    public IEnumerable<InteractableBehaviour> GetInteractables()
    {
        return FindInteractables(transform.position, Radius, Mask)
            .OrderBy(item => Vector3.Distance(transform.position, item.transform.position));
    }

    public void Interact()
    {
        if (SelectedInteractable != null)
        {
            SelectedInteractable.Interact(this.gameObject);
        }
    }

    #region Unity Methods

    private void Awake()
    {
        _interactionMonitoringUniTaskCorutine = new(InteractableMonitoring);
    }

    private void OnEnable()
    {
        _interactionMonitoringUniTaskCorutine.Run();
    }

    private void OnDisable()
    {
        _interactionMonitoringUniTaskCorutine.Stop();
    }

    private void OnDestroy()
    {
        _interactionMonitoringUniTaskCorutine.Dispose();
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, Radius);
    }
    #endregion
}