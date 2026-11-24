using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachinePositionComposer))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private const float UiCoverOffsetFactor = 0.5f;

    [SerializeField, Range(0f, 0.9f)] private float bottomUiRatio = 0.1f;

    [SerializeField] private Transform _target;

    private CinemachineCamera _virtualCamera;
    private CinemachinePositionComposer _composer;

    public CinemachineCamera VirtualCamera => _virtualCamera;

    public Transform Target
    {
        get
        { 
            return _target;
        }
        set
        {
            _target = value;
            _virtualCamera.Follow = _target;
            _virtualCamera.PreviousStateIsValid = false;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CameraController] 이미 메인 카메라가 있어서 이 컴포넌트는 꺼집니다.");
            enabled = false;
            return;
        }

        Instance = this;

        _virtualCamera = GetComponent<CinemachineCamera>();
        _composer = GetComponent<CinemachinePositionComposer>();

        ApplyBottomUiRatio();

        if (_target != null)
        {
            Target = _target;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ApplyBottomUiRatio()
    {
        if (_composer == null) { return; }

        ScreenComposerSettings composition = _composer.Composition;

        composition.ScreenPosition.y = -bottomUiRatio * UiCoverOffsetFactor;
        _composer.Composition = composition;
    }
}
