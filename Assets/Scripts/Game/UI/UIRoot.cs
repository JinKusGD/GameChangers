using UnityEngine;

public class UIRoot : MonoBehaviour
{
    [SerializeField] private Transform _hudRoot;
    [SerializeField] private Transform _popupRoot;
    [SerializeField] private Transform _systemRoot;

    public Transform GetRoot(UICanvasType canvasType)
    {
        switch (canvasType)
        {
            case UICanvasType.HUD:
                return _hudRoot;
            case UICanvasType.Popup:
                return _popupRoot;
            case UICanvasType.System:
                return _systemRoot;
            default:
                return _hudRoot;
        }
    }
}
