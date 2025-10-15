using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class ViewManager
{
    private Canvas _mainCanvas;
    private MonoCanvasView _focusedView;

    private readonly DiContainer _container;

    public ViewManager(Canvas mainCanvas, DiContainer container)
    {
        _mainCanvas = mainCanvas;
        _container = container;
    }

    public AnimationScope Animate<T>(ViewAnimation<T> animation, CancellationToken token = default) where T : MonoCanvasView
    {
        AnimationScope scope = new(animation, token);
        return scope;
    }

    public ViewCreationScope<T> CreateView<T>(IFactory<T> factory, Transform parent = null) where T : MonoCanvasView
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        T view = factory.Create();

        AttachToParent(view, parent);

        return new ViewCreationScope<T>(view);
    }

    public ViewCreationScope<T> CreateView<T>(Func<T> factory, Transform parent = null) where T : MonoCanvasView
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        T view = factory.Invoke();

        AttachToParent(view, parent);

        return new ViewCreationScope<T>(view);
    }

    public MonoCanvasView InstantiateViewFromPrefab(GameObject prefab, Transform parent = null)
    {
        if (prefab == null)
            throw new ArgumentNullException(nameof(prefab));

        GameObject instance = _container.InstantiatePrefab(prefab);
        MonoCanvasView view = instance.GetComponent<MonoCanvasView>();

        if (view == null)
            throw new InvalidOperationException("Prefab does not contain MonoCanvasView component");

        AttachToParent(view, parent);

        return view;
    }

    private void AttachToParent(MonoCanvasView view, Transform parent)
    {
        if (parent != null)
            view.transform.SetParent(parent, false);
        else if (_mainCanvas != null)
            view.transform.SetParent(_mainCanvas.transform, false);
    }

    public void SetFocus(MonoCanvasView newFocus)
    {
        if (_focusedView == newFocus)
            return;

        _focusedView?.Defocus();
        _focusedView = newFocus;
        _focusedView.Focus();
    }

    public void ClearFocus()
    {
        _focusedView?.Defocus();
        _focusedView = null;
    }

    public MonoCanvasView GetFocusedView()
    {
        return _focusedView;
    }
}
