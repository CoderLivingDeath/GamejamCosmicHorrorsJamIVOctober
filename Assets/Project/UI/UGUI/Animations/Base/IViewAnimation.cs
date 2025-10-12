using Cysharp.Threading.Tasks;
using System.Threading;

public interface IViewAnimation : IScriptableAnimation
{
    MonoCanvasView View { get; }
}
