using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class MonoInteractable : MonoBehaviour
{
    public int Priority => _priority;
    public bool BoundsSupport => _boundsSupport;
    public Bounds? InteractionBounds => _interactionBounds;

    #region  Unity Internal
    [SerializeField]
    private int _priority = 0;

    [SerializeField]
    private bool _boundsSupport = false;

    [SerializeField]
    private Bounds _interactionBounds;

    [SerializeField]
    private bool _canInteract = true;

    public UnityEvent OnInteractEvent;

#if UNITY_EDITOR
    [SerializeField]
    [SelectionPopup(nameof(strs), callbackName: nameof(OnItemSelected), placeholder: "{select}")]

    private string Interaction;

    public SelectItem[] strs => GetSelectedItems().ToArray();

#endif

    private void OnDestroy()
    {
        OnInteractEvent.RemoveAllListeners();
    }

    #endregion 

    public Assembly GetAssembly()
    {
        return this.GetType().Assembly;
    }

    public bool CanInteractWithPosition(Vector3 interactorPosition)
    {
        return _canInteract && this.enabled && IsWithinInteractionBounds(interactorPosition);
    }

    public bool IsWithinInteractionBounds(Vector3 interactorPosition)
    {
        if (_interactionBounds != null && BoundsSupport)
        {
            // Получаем локальную позицию взаимодействующего, преобразуя из мировых координат
            Vector3 localPos = transform.InverseTransformPoint(interactorPosition);

            return _interactionBounds.Contains(localPos);
        }
        return true;
    }


    private IEnumerable<Type> GetBehavioursByAttribute(Type attributeType, Assembly assembly)
    {
        if (assembly == null)
            yield break;

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null).ToArray();
        }

        foreach (Type type in types)
        {
            if (type == null)
                continue;

            if (type.IsSubclassOf(typeof(MonoBehaviour)) &&
                !type.IsAbstract &&
                Attribute.IsDefined(type, attributeType, false))
            {
                yield return type;
            }
        }
    }

    private Type GetBehaviourByNameAndAttribute(string name, Type attributeType, Assembly assembly)
    {
        if (assembly == null || string.IsNullOrEmpty(name) || attributeType == null)
            return null;

        try
        {
            return assembly.GetTypes()
                .Where(t => t != null
                            && t.IsClass
                            && !t.IsAbstract
                            && t.IsSubclassOf(typeof(MonoBehaviour))
                            && t.Name == name
                            && Attribute.IsDefined(t, attributeType, false))
                .FirstOrDefault();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types
                .Where(t => t != null
                            && t.IsClass
                            && !t.IsAbstract
                            && t.IsSubclassOf(typeof(MonoBehaviour))
                            && t.Name == name
                            && Attribute.IsDefined(t, attributeType, false))
                .FirstOrDefault();
        }
    }

    private IEnumerable<Type> GetInteractableTypes()
    {
        foreach (var item in GetBehavioursByAttribute(typeof(InteractableComponentAttribute), GetAssembly()))
        {
            yield return item;
        }
    }

    private IEnumerable<SelectItem> GetSelectedItems()
    {
        foreach (var type in GetInteractableTypes())
        {
            var exists = gameObject.GetComponent(type) != null;
            var name = type.Name;
            var displayName = type.Name;

            bool isSelected = false;
            bool isActive = !exists;

            yield return new SelectItem(name, displayName, isSelected, isActive);
        }
    }
    private Type GetInteractableBehaviourByName(string value)
    {
        return GetBehaviourByNameAndAttribute(value, typeof(InteractableComponentAttribute), GetAssembly());
    }

    private void OnItemSelected(string value)
    {
        Type behaviour = GetInteractableBehaviourByName(value);

        if (behaviour == null)
        {
            Debug.LogWarning($"��������� � ������ {value} �� ������.");
            return;
        }

        if (gameObject.GetComponent(behaviour) == null)
        {
            gameObject.AddComponent(behaviour);
        }
    }

    public bool CanInteract()
    {
        return _canInteract && this.enabled;
    }

    public void Interact(GameObject sender)
    {
        if (!CanInteract()) return;

        var interactables = GetComponents<MonoInteractableHandlerBase>();

        InteractionContext context = new(this, sender);

        foreach (var interactable in interactables)
        {
            Debug.Log(gameObject.name);
            interactable.HandleInteract(context);
        }

        OnInteractEvent?.Invoke();
    }
}

public readonly struct InteractionContext
{
    public readonly MonoInteractable Interactable;
    public readonly GameObject Interactor;

    public InteractionContext(MonoInteractable interactable, GameObject interactor)
    {
        Interactable = interactable;
        Interactor = interactor;
    }
}