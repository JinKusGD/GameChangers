using System.Collections.Generic;
using UnityEngine;

public class StageField : MonoBehaviour
{
    [Tooltip("필드가 차지할 최대 크기 (월드 단위). 칸이 정사각형이 되도록 이 안에 맞춘다.")]
    [SerializeField] private Vector2 fieldSize = new Vector2(5.4f, 5.4f);

    [Tooltip("Tier 1부터 순서대로 쓸 프리팹 (크기 1x1 기준). 비어 있으면 색 사각형으로 대신 만든다.")]
    [SerializeField] private GameObject[] tierPrefabs;

    [Tooltip("칸 크기 대비 오브젝트 크기. 1이면 칸을 꽉 채운다.")]
    [SerializeField, Range(0.1f, 1f)] private float objectScale = 0.9f;

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
    private GameManager gameManager;

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

        float cell = Mathf.Min(fieldSize.x / template.Columns, fieldSize.y / template.Rows);
        Vector2 gridSize = new Vector2(template.Columns * cell, template.Rows * cell);

        foreach (StageTemplateSlot slot in gameManager.StagePlacements)
        {
            GameObject obj = CreateObject(slot.Tier);
            obj.name = $"Tier{slot.Tier}";
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = Vector2.Scale(slot.NormalizedPos - new Vector2(0.5f, 0.5f), gridSize);
            obj.transform.localScale = Vector3.one * (cell * objectScale);

            spawned.Add(obj);
        }
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, fieldSize);
    }
#endif
}
