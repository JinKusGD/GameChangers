using UnityEngine;

public class GameSceneInitializer : MonoBehaviour
{
    [SerializeField] private PlayerView _playerPrefab;

    private void Start()
    {
        if (!UnityUtility.CheckReference(_playerPrefab, nameof(_playerPrefab), this)) { return; }

        PlayerView player = SpawnPlayer();
        AttachCamera(player.transform);

        Destroy(gameObject);
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

        if (gameManager == null || gameManager.Data == null) { return Vector2.zero; }

        if (gameManager.Data.StageSave.TryGetPlayerPosition(out Vector2 savedPosition)) { return savedPosition; }

        StageInfo stageInfo = DataManager.Instance.GetStageInfo(gameManager.Chapter, gameManager.Stage);

        return stageInfo != null ? stageInfo.FieldPosition : Vector2.zero;
    }

    private void AttachCamera(Transform target)
    {
        CameraController cameraController = CameraController.Instance;

        if (cameraController == null)
        {
            DebugUtility.LogError("[GameSceneInitializer] CameraController가 없어 카메라가 플레이어를 따라가지 않습니다.", this);
            return;
        }

        cameraController.Target = target;
    }
}
