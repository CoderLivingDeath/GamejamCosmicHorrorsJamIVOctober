using System;
using UnityEngine;

public abstract class MonoCanvasView : MonoBehaviour
{
    public RectTransform RectTransform => (RectTransform)transform;

    public Rect Rect => RectTransform.rect;

    public bool IsAnimating { get; set; }

    // ��������� ���� � �������, ������, �������
    private bool _isOpened = false;
    private bool _isClosed = false;

    // ������� ���������� ����� ����
    public event Action OnOpening;
    public event Action OnOpened;
    public event Action OnHiding;
    public event Action OnHidden;
    public event Action OnClosing;
    public event Action OnClosed;

    /// <summary>
    /// ��������� ���� (������ ������ ��������) � �������� �������.
    /// </summary>
    public virtual void Open()
    {
        if (_isClosed) return; // ���� ���� �������, �� ��������� ��������

        OnOpening?.Invoke();

        gameObject.SetActive(true);
        _isOpened = true;

        OnOpened?.Invoke();
    }

    /// <summary>
    /// �������� ���� (������������ ������) � �������� �������.
    /// </summary>
    public virtual void Hide()
    {
        if (!_isOpened) return; // ���� ���� �� ���� �������, ����� ������������

        OnHiding?.Invoke();

        gameObject.SetActive(false);
        _isOpened = false;

        OnHidden?.Invoke();
    }

    /// <summary>
    /// ��������� ���� � ����� ������� �������� � ����������� �������.
    /// </summary>
    public virtual void Close()
    {
        if (_isClosed) return;

        OnClosing?.Invoke();

        _isClosed = true;
        _isOpened = false;

        OnClosed?.Invoke();

        Destroy(gameObject);
    }

    /// <summary>
    /// ���������, ������� �� ����.
    /// </summary>
    public bool IsOpen => _isOpened && !_isClosed;

    /// <summary>
    /// ���������, ������� �� ����.
    /// </summary>
    public bool IsClosed => _isClosed;
}
