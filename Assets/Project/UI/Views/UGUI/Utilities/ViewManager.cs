using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class ViewManager
{
    private Canvas _mainCanvas;

    public ViewManager(Canvas mainCanvas)
    {
        _mainCanvas = mainCanvas;
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

        if (parent != null)
            view.transform.SetParent(parent, false);
        else if (_mainCanvas != null)
            view.transform.SetParent(_mainCanvas.transform, false);

        return new ViewCreationScope<T>(view);
    }
}
