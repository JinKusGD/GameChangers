using System.Collections.Generic;
using UnityEngine;

public class StageField : MonoBehaviour
{
    [SerializeField] private GameObject[] tierPrefabs;

    [SerializeField, Min(1)] private int blocksPerCell = 5;

    private const float BlockWorldSize = 1f;

    private static readonly Color[] FallbackColors =
    {
        new Color(0.55f, 0.52f, 0.50f), // 1
        new Color(0.80f, 0.48f, 0.28f), // 2
        new Color(0.95f, 0.80f, 0.25f), // 3
        new Color(0.45f, 0.80f, 0.95f), // 4
        new Color(0.70f, 0.45f, 0.90f), // 5
        new Color(0.40f, 0.85f, 0.45f), // 6
        new Color(0.95f, 0.45f, 0.55f), // 7
        new Color(0.30f, 0.40f, 0.85f), // 8
        new Color(0.15f, 0.15f, 0.15f), // 9
    };

    private static Sprite fallbackSprite;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly List<Vector2> spawnedPositions = new List<Vector2>();
    private GameManager gameManager;

    private Vector2 gridSize;

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("[StageField] GameManager가 없습니다.");
            return;
        }

        gameManager.StagePlacementsChanged += Build;
        Build();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.StagePlacementsChanged -= Build;
        }
    }

    public void Build()
    {
        Clear();

        StageTemplate template = gameManager.CurrentTemplate;

        if (template == null || template.Columns == 0 || template.Rows == 0) { return; }

        Vector2 cellSize = template.CellSize;
        Vector2 blockSize = cellSize / blocksPerCell;

        foreach (StageTemplateSlot slot in gameManager.StagePlacements)
        {
            Vector2 cellMin = slot.NormalizedPos - cellSize * 0.5f;

            for (int y = 0; y < blocksPerCell; y++)
            {
                for (int x = 0; x < blocksPerCell; x++)
                {
                    GameObject obj = CreateObject(slot.Tier);
                    obj.name = $"Tier{slot.Tier}";
                    obj.transform.SetParent(transform, false);

                    spawned.Add(obj);
                    spawnedPositions.Add(cellMin + Vector2.Scale(new Vector2(x + 0.5f, y + 0.5f), blockSize));
                }
            }
        }

        Layout();
    }

    public void Clear()
    {
        foreach (GameObject obj in spawned)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        spawned.Clear();
        spawnedPositions.Clear();
        gridSize = Vector2.zero;
    }

    private void Layout()
    {
        StageTemplate template = gameManager.CurrentTemplate;

        if (template == null || template.Columns == 0) { return; }

        int blockColumns = template.Columns * blocksPerCell;
        int blockRows = template.Rows * blocksPerCell;

        gridSize = new Vector2(blockColumns, blockRows) * BlockWorldSize;

        for (int i = 0; i < spawned.Count; i++)
        {
            Transform t = spawned[i].transform;
            t.localPosition = Vector2.Scale(spawnedPositions[i] - new Vector2(0.5f, 0.5f), gridSize);
            t.localScale = Vector3.one * BlockWorldSize;
        }
    }

    private GameObject CreateObject(int tier)
    {
        int index = tier - 1;

        if (tierPrefabs != null && index < tierPrefabs.Length && tierPrefabs[index] != null)
        {
            return Instantiate(tierPrefabs[index]);
        }

        GameObject obj = new GameObject();
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = GetFallbackSprite();
        renderer.color = FallbackColors[Mathf.Clamp(index, 0, FallbackColors.Length - 1)];

        return obj;
    }

    private static Sprite GetFallbackSprite()
    {
        if (fallbackSprite == null)
        {
            Texture2D texture = Texture2D.whiteTexture;
            fallbackSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.width);
        }

        return fallbackSprite;
    }
}
