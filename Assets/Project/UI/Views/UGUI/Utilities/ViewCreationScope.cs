using System.Threading;

public class ViewCreationScope<T> where T : MonoCanvasView
{
    public readonly T View;

    public ViewCreationScope(T view)
    {
        this.View = view;
    }

    public AnimationScope WithAnimataion(IViewAnimation animation, CancellationToken token = default)
    {
        AnimationScope scope = new(animation, token);
        return scope;
    }

    public void Open()
    {
        View.Open();
    }

    public void Close()
    {
        View.Close();
    }
}