using System;
using UnityEngine;

public abstract class MonoCanvasView : MonoBehaviour
{
    public RectTransform RectTransform => (RectTransform)transform;
    public Rect Rect => RectTransform.rect;
    public bool IsAnimating { get; set; }

    private bool _isOpened = false;
    private bool _isClosed = false;
    private bool _hasFocus = false;

    public event Action OnOpening;
    public event Action OnOpened;
    public event Action OnHiding;
    public event Action OnHidden;
    public event Action OnClosing;
    public event Action OnClosed;

    /// <summary>
    /// Получить или установить фокус окна
    /// </summary>
    public bool HasFocus
    {
        get => _hasFocus;
        protected set
        {
            if (_hasFocus == value) return;
            _hasFocus = value;
            if (value)
                OnFocus();
            else
                OnDefocus();
        }
    }

    /// <summary>
    /// Открыть окно и установить фокус
    /// </summary>
    public virtual void Open()
    {
        if (_isClosed) return;

        OnOpening?.Invoke();

        gameObject.SetActive(true);
        _isOpened = true;

        HasFocus = true; // ставим фокус

        OnOpened?.Invoke();
    }

    /// <summary>
    /// Скрыть окно, снять фокус
    /// </summary>
    public virtual void Hide()
    {
        if (!_isOpened) return;

        OnHiding?.Invoke();

        HasFocus = false; // снимаем фокус
        gameObject.SetActive(false);
        _isOpened = false;

        OnHidden?.Invoke();
    }

    /// <summary>
    /// Закрыть окно и снять фокус
    /// </summary>
    public virtual void Close()
    {
        if (_isClosed) return;

        OnClosing?.Invoke();

        HasFocus = false;
        _isClosed = true;
        _isOpened = false;

        OnClosed?.Invoke();

        Destroy(gameObject);
    }

    /// <summary>
    /// Установка окна в фокус
    /// </summary>
    public virtual void Focus()
    {
        if (_hasFocus) return;
        _hasFocus = true;
        OnFocus();
    }

    /// <summary>
    /// Снятие фокуса с окна
    /// </summary>
    public virtual void Defocus()
    {
        if (!_hasFocus) return;
        _hasFocus = false;
        OnDefocus();
    }

    /// <summary>
    /// Вызывается при установке фокуса
    /// </summary>
    protected virtual void OnFocus()
    {
        // Переключить input обработку, подсветку и т.п.
    }

    /// <summary>
    /// Вызывается при снятии фокуса
    /// </summary>
    protected virtual void OnDefocus()
    {
        // Отключить input обработку, убрать подсветку и т.п.
    }
}
