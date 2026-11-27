using UnityEngine;
using UnityEngine.UI;

public class MinimapView : MonoBehaviour
{
    private const string Tag = "MinimapView";
    private const int ViewCellCount = 24;
    private const int BorderSize = 1;

    private static readonly Color32 EmptyColor = new Color32(30, 36, 52, 255);

    [SerializeField] private RawImage _mapImage;

    private Transform _player;
    private StageManager _stageManager;
    private Texture2D _texture;
    private Color32[] _pixels;
    private int _gridSize;
    private int _textureSize;
    private bool _isDirty;

    private void OnDestroy()
    {
        Unbind();
    }

    private void LateUpdate()
    {
        if (_stageManager == null) { return; }

        if (_isDirty)
        {
            _isDirty = false;
            Redraw();
        }

        if (_player == null) { return; }

        FollowPlayer();
    }

    public void Bind(Transform player)
    {
        if (!UnityUtility.CheckReference(_mapImage, nameof(_mapImage), this)) { return; }

        StageManager stageManager = StageManager.Instance;

        if (stageManager == null)
        {
            DebugUtility.LogError($"[{Tag}] StageManager가 없습니다.", this);
            return;
        }

        Unbind();

        _player = player;
        _stageManager = stageManager;
        _stageManager.BlocksChanged += OnBlocksChanged;

        CreateTexture(_stageManager.GridSize);
        _isDirty = true;
    }

    private void Unbind()
    {
        if (_stageManager != null)
        {
            _stageManager.BlocksChanged -= OnBlocksChanged;
            _stageManager = null;
        }

        _player = null;
        DestroyTexture();
    }

    private void OnBlocksChanged()
    {
        _isDirty = true;
    }

    private void CreateTexture(int gridSize)
    {
        _gridSize = gridSize;

        if (_gridSize == 0) { return; }

        _textureSize = _gridSize + BorderSize * 2;

        _texture = new Texture2D(_textureSize, _textureSize, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        _mapImage.texture = _texture;

        CreatePixels();
    }

    private void CreatePixels()
    {
        _pixels = new Color32[_textureSize * _textureSize];

        for (int i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = EmptyColor;
        }
    }

    private void DestroyTexture()
    {
        if (_texture == null) { return; }

        if (_mapImage != null)
        {
            _mapImage.texture = null;
        }

        Destroy(_texture);
        _texture = null;
        _pixels = null;
        _gridSize = 0;
    }

    private void Redraw()
    {
        if (_texture == null) { return; }

        for (int row = 0; row < _gridSize; row++)
        {
            for (int column = 0; column < _gridSize; column++)
            {
                Color32 pixel = EmptyColor;

                if (_stageManager.TryGetCellColor(column, row, out Color color))
                {
                    pixel = color;
                }

                _pixels[(row + BorderSize) * _textureSize + column + BorderSize] = pixel;
            }
        }

        _texture.SetPixels32(_pixels);
        _texture.Apply(false);
    }

    private void FollowPlayer()
    {
        if (_texture == null) { return; }

        Vector2 normalized = _stageManager.GetNormalizedPosition(_player.position);

        float centerU = (normalized.x * _gridSize + BorderSize) / _textureSize;
        float centerV = (normalized.y * _gridSize + BorderSize) / _textureSize;
        float viewSize = (float)ViewCellCount / _textureSize;

        _mapImage.uvRect = new Rect(centerU - viewSize * 0.5f, centerV - viewSize * 0.5f, viewSize, viewSize);
    }
}
