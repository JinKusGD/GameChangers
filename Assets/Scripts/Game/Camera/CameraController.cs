using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachinePositionComposer))]
public class CameraController : MonoBehaviour
{
    private const float UiCoverOffsetFactor = 0.5f;

    [SerializeField, Range(0f, 0.9f), FormerlySerializedAs("bottomUiRatio")] private float _bottomUiRatio = 0.1f;
    [SerializeField] private Transform _target;

    private CinemachineCamera _virtualCamera;
    private CinemachinePositionComposer _composer;

    public static CameraController Instance { get; private set; }

    public CinemachineCamera VirtualCamera => _virtualCamera;

    public Transform Target
    {
        get => _target;
        set
        {
            _target = value;
            ApplyTarget();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DebugUtility.LogWarning("[CameraController] 이미 메인 카메라가 있어서 이 컴포넌트는 꺼집니다.", this);
            enabled = false;
            return;
        }

        Instance = this;

        _virtualCamera = GetComponent<CinemachineCamera>();
        _composer = GetComponent<CinemachinePositionComposer>();

        ApplyBottomUiRatio();
        ApplyTarget();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ApplyTarget()
    {
        if (_virtualCamera == null || _target == null) { return; }

        _virtualCamera.Follow = _target;
        _virtualCamera.PreviousStateIsValid = false;
    }

    private void ApplyBottomUiRatio()
    {
        if (_composer == null) { return; }

        ScreenComposerSettings composition = _composer.Composition;
        composition.ScreenPosition.y = -_bottomUiRatio * UiCoverOffsetFactor;
        _composer.Composition = composition;
    }
}
