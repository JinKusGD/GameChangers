using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Block))]
public class BlockHitEffect : MonoBehaviour
{
    private const float SettledSquash = 0.001f;
    private const float SettledSquashVelocity = 0.01f;

    [Header("Squash")]
    [SerializeField, Min(0f), FormerlySerializedAs("squashImpulse")] private float _squashImpulse = 4f;
    [SerializeField, Min(0f), FormerlySerializedAs("stiffness")] private float _stiffness = 300f;
    [SerializeField, Min(0f), FormerlySerializedAs("damping")] private float _damping = 14f;
    [SerializeField, Range(0f, 0.5f), FormerlySerializedAs("maxSquash")] private float _maxSquash = 0.3f;

    [Header("Shake")]
    [SerializeField, Min(0f), FormerlySerializedAs("shakeStrength")] private float _shakeStrength = 0.06f;
    [SerializeField, Min(0f), FormerlySerializedAs("shakeDuration")] private float _shakeDuration = 0.12f;

    private Block _block;

    private Vector3 _baseScale;
    private Vector3 _basePosition;

    private float _squash;
    private float _squashVelocity;
    private float _shakeTimer;
    private bool _isPlaying;

    private void Awake()
    {
        _block = GetComponent<Block>();

        _block.Damaged += OnDamaged;
        _block.Cleared += OnCleared;

        enabled = false;
    }

    private void OnDestroy()
    {
        if (_block != null)
        {
            _block.Damaged -= OnDamaged;
            _block.Cleared -= OnCleared;
        }
    }

    private void OnDisable()
    {
        if (_isPlaying)
        {
            Restore();
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdateSquash(deltaTime);
        UpdateShake(deltaTime);

        if (IsSettled())
        {
            Restore();
        }
    }

    private void OnDamaged(Block _, double damage)
    {
        if (!_isPlaying)
        {
            Begin();
        }

        _squashVelocity += _squashImpulse;
        _shakeTimer = _shakeDuration;
    }

    private void OnCleared(Block _)
    {
        if (_isPlaying)
        {
            Restore();
        }
    }

    private void Begin()
    {
        _baseScale = transform.localScale;
        _basePosition = transform.localPosition;

        _isPlaying = true;
        enabled = true;
    }

    private void UpdateSquash(float deltaTime)
    {
        float acceleration = -_stiffness * _squash - _damping * _squashVelocity;
        _squashVelocity += acceleration * deltaTime;
        _squash = Mathf.Clamp(_squash + _squashVelocity * deltaTime, -_maxSquash, _maxSquash);

        transform.localScale = Vector3.Scale(_baseScale, new Vector3(1f + _squash, 1f - _squash, 1f));
    }

    private void UpdateShake(float deltaTime)
    {
        Vector3 shakeOffset = Vector3.zero;

        if (_shakeTimer > 0f)
        {
            _shakeTimer -= deltaTime;

            float strength = _shakeStrength * Mathf.Clamp01(_shakeTimer / _shakeDuration);
            shakeOffset = Random.insideUnitCircle * strength;
        }

        transform.localPosition = _basePosition + shakeOffset;
    }

    private bool IsSettled()
    {
        bool isSquashSettled = Mathf.Abs(_squash) < SettledSquash && Mathf.Abs(_squashVelocity) < SettledSquashVelocity;

        return isSquashSettled && _shakeTimer <= 0f;
    }

    private void Restore()
    {
        _squash = 0f;
        _squashVelocity = 0f;
        _shakeTimer = 0f;
        _isPlaying = false;

        transform.localScale = _baseScale;
        transform.localPosition = _basePosition;

        enabled = false;
    }
}
