using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameSceneInitializer : MonoBehaviour
{
    private const string Tag = "GameSceneInitializer";

    [SerializeField] private PlayerView _playerPrefab;

    private void Start()
    {
        if (!UnityUtility.CheckReference(_playerPrefab, nameof(_playerPrefab), this)) { return; }

        PlayerView player = SpawnPlayer();
        AttachCamera(player.transform);
        ShowHudAsync(player.transform).Forget();

        Destroy(gameObject);
    }

    private static async UniTask ShowHudAsync(Transform player)
    {
        UIManager uiManager = UIManager.Instance;

        if (uiManager == null)
        {
            DebugUtility.LogError($"[{Tag}] UIManager가 없어 HUD를 표시하지 않습니다.");
            return;
        }

        HudView hudView = await uiManager.ShowAsync<HudView>();

        if (hudView == null) { return; }

        hudView.SetPlayer(player);
    }

    private PlayerView SpawnPlayer()
    {
        Transform prefabTransform = _playerPrefab.transform;

        Vector2 spawnPosition = GetSpawnPosition();

        Vector3 position = new Vector3(spawnPosition.x, spawnPosition.y, prefabTransform.position.z);

        return Instantiate(_playerPrefab, position, prefabTransform.rotation);
    }

    private static Vector2 GetSpawnPosition()
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null || !gameManager.IsLoaded) { return Vector2.zero; }

        if (gameManager.StageSave.TryGetPlayerPosition(out Vector2 savedPosition)) { return savedPosition; }

        StageModel stageModel = ModelManager.Instance.StageModel;
        StageInfo stageInfo = DataManager.Instance.GetStageInfo(stageModel.Chapter, stageModel.Stage);

        return stageInfo != null ? stageInfo.FieldPosition : Vector2.zero;
    }

    private void AttachCamera(Transform target)
    {
        CameraController cameraController = CameraController.Instance;

        if (cameraController == null)
        {
            DebugUtility.LogError($"[{Tag}] CameraController가 없어 카메라가 플레이어를 따라가지 않습니다.", this);
            return;
        }

        cameraController.Target = target;
    }
}
