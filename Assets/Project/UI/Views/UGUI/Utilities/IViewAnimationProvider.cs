public interface IViewAnimationProvider<T> where T : MonoCanvasView
{
    ViewAnimation<T> ProvideAnimation(T view);
}