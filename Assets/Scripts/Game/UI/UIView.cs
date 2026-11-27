using UnityEngine;

public abstract class UIView : MonoBehaviour
{
    [SerializeField] private UICanvasType _canvasType;

    public UICanvasType CanvasType => _canvasType;
    public bool IsShown => gameObject.activeSelf;

    public bool Create()
    {
        return OnCreate();
    }

    public void Show()
    {
        if (IsShown) { return; }

        gameObject.SetActive(true);
        OnShow();
    }

    public void Hide()
    {
        if (!IsShown) { return; }

        OnHide();
        gameObject.SetActive(false);
    }

    protected virtual bool OnCreate()
    {
        return true;
    }

    protected virtual void OnShow() { }

    protected virtual void OnHide() { }
}
