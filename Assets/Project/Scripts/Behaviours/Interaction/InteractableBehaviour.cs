using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class InteractableBehaviour : InteractableBehaviourBase
{
    public UnityEvent OnInteractEvent;

    public bool CanInteract => _canInteract;

    [SerializeField]
    private bool _canInteract = true;

    [SerializeField]
    [SelectionPopup(nameof(strs), callbackName: nameof(OnItemSelected), placeholder: "{select}")]
    private string Interaction;

    public SelectItem[] strs => GetSelectedItems().ToArray();

    public Assembly GetAssembly()
    {
        return this.GetType().Assembly;
    }

    /// <summary>
    /// ������� ��� ����, �������������� �� MonoBehaviour, ���������� �������� ���������, � ��������� ������.
    /// </summary>
    /// <param name="attributeType">��� ��������-������� (typeof(MyAttr)).</param>
    /// <param name="assembly">��������� ������ ��� ������.</param>
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

    /// <summary>
    /// ������� ��� MonoBehaviour � ������ �� ����� � ��������.
    /// </summary>
    /// <param name="name">��� ���� (��� namespace).</param>
    /// <param name="attributeType">��� �������� (typeof(MyAttribute)).</param>
    /// <param name="assembly">������ ��� ������.</param>
    /// <returns>Type ���������� ����������, ���� null ���� �� ������.</returns>
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

    public bool CanIneract()
    {
        return _canInteract && this.enabled;
    }

    public override void Interact(GameObject sender)
    {
        if (!CanIneract()) return;

        var interactables = GetComponents<MonoInteractableHandlerBase>();

        InteractionContext context = new(this, sender);

        foreach(var interactable in interactables)
        {
            interactable.HandleInteract(context);
        }

        OnInteractEvent?.Invoke();
    }

    private void OnDestroy()
    {
        OnInteractEvent.RemoveAllListeners();
    }
}

public readonly struct InteractionContext
{
    public readonly InteractableBehaviour Interactable;
    public readonly GameObject Interactor;

    public InteractionContext(InteractableBehaviour interactable, GameObject interactor)
    {
        Interactable = interactable;
        Interactor = interactor;
    }
}

public abstract class InteractableBehaviourBase : MonoBehaviour
{

    public virtual void Interact(GameObject sender)
    {

    }
}
