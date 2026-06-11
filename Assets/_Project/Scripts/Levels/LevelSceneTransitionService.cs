using System.Collections;
using System.Collections.Generic;
using IdleonGame.Cameras;
using IdleonGame.Map;
using IdleonGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Levels
{
    [DisallowMultipleComponent]
    public sealed class LevelSceneTransitionService : MonoBehaviour
    {
        public static LevelSceneTransitionService Instance { get; private set; }

        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private BattleSceneController battleController;
        [SerializeField] private LoadingOverlay loadingOverlay;
        [SerializeField] private string initialLevelId = "level1_1";
        [SerializeField] private KeyCode debugSwitchKey = KeyCode.I;

        private Scene currentLevelScene;
        private LevelDefinition currentLevel;
        private bool isSwitching;
        private readonly List<MapPortal> portals = new();

        public Scene CurrentLevelScene => currentLevelScene;
        public LevelDefinition CurrentLevel => currentLevel;
        public bool IsSwitching => isSwitching;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            FindReferences();
        }

        private IEnumerator Start()
        {
            FindReferences();
            battleController?.EnsurePlayer();

            var targetLevel = levelDatabase != null ? levelDatabase.GetDefaultLevel() : LevelDatabase.Find(initialLevelId);
            if (targetLevel != null)
            {
                yield return SwitchToLevelRoutine(targetLevel.LevelId, targetLevel.DefaultSpawnPointId);
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(debugSwitchKey))
            {
                var nextLevelId = currentLevel != null && currentLevel.LevelId == "level1_1" ? "level1_2" : "level1_1";
                SwitchToLevel(nextLevelId);
            }
        }

        public void SwitchToLevel(string levelId)
        {
            SwitchToLevel(levelId, null);
        }

        public void SwitchToLevel(string levelId, string spawnPointId)
        {
            if (isSwitching)
            {
                return;
            }

            StartCoroutine(SwitchToLevelRoutine(levelId, spawnPointId));
        }

        private IEnumerator SwitchToLevelRoutine(string levelId, string spawnPointId)
        {
            FindReferences();
            var targetLevel = levelDatabase != null ? levelDatabase.GetLevel(levelId) : LevelDatabase.Find(levelId);
            if (targetLevel == null)
            {
                Debug.LogWarning($"Level transition failed. Unknown level id: {levelId}");
                yield break;
            }

            isSwitching = true;
            loadingOverlay?.Show();
            var previousLevelId = currentLevel != null ? currentLevel.LevelId : null;

            if (currentLevelScene.IsValid() && currentLevelScene.isLoaded)
            {
                NotifyLevelSceneWillUnload(currentLevelScene);
            }

            var loadOperation = SceneManager.LoadSceneAsync(targetLevel.SceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Debug.LogWarning($"Level transition failed. Scene is not in build settings: {targetLevel.SceneName}");
                loadingOverlay?.Hide();
                isSwitching = false;
                yield break;
            }

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            var loadedScene = SceneManager.GetSceneByName(targetLevel.SceneName);
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                Debug.LogWarning($"Level transition failed. Scene did not load: {targetLevel.SceneName}");
                loadingOverlay?.Hide();
                isSwitching = false;
                yield break;
            }

            SceneManager.SetActiveScene(loadedScene);
            NotifyLevelSceneLoaded(loadedScene);
            MovePlayerToLevelSpawn(targetLevel, spawnPointId, previousLevelId, loadedScene);
            RefreshCameraForLoadedLevel();

            if (currentLevelScene.IsValid() && currentLevelScene.isLoaded && currentLevelScene != loadedScene)
            {
                var unloadOperation = SceneManager.UnloadSceneAsync(currentLevelScene);
                while (unloadOperation != null && !unloadOperation.isDone)
                {
                    yield return null;
                }
            }

            currentLevelScene = loadedScene;
            currentLevel = targetLevel;

            yield return new WaitForSeconds(1f);
            loadingOverlay?.Hide();
            isSwitching = false;
        }

        private void MovePlayerToLevelSpawn(LevelDefinition level, string spawnPointId, string previousLevelId, Scene loadedScene)
        {
            if (battleController == null)
            {
                return;
            }

            if (TryGetPreviousLevelPortalPosition(previousLevelId, loadedScene, out var portalPosition))
            {
                battleController.MovePlayerTo(portalPosition);
                return;
            }

            var targetSpawnPointId = string.IsNullOrEmpty(spawnPointId) ? level.DefaultSpawnPointId : spawnPointId;
            var position = level.DefaultSpawnPosition;
            var mapController = LevelSceneReferenceResolver.FindInActiveScene<BattleMapController>();
            var mapDefinition = mapController != null ? mapController.MapDefinition : null;
            if (mapDefinition != null)
            {
                foreach (var spawnPoint in mapDefinition.SpawnPoints)
                {
                    if (spawnPoint != null && spawnPoint.spawnPointId == targetSpawnPointId)
                    {
                        position = spawnPoint.position;
                        break;
                    }
                }
            }

            battleController.MovePlayerTo(position);
        }

        private bool TryGetPreviousLevelPortalPosition(string previousLevelId, Scene loadedScene, out Vector2 position)
        {
            position = default;
            if (string.IsNullOrEmpty(previousLevelId))
            {
                return false;
            }

            LevelSceneReferenceResolver.FindAllInScene(loadedScene, portals);
            foreach (var portal in portals)
            {
                if (portal != null && portal.TargetLevelId == previousLevelId)
                {
                    position = portal.WorldPosition;
                    portals.Clear();
                    return true;
                }
            }

            portals.Clear();
            return false;
        }

        private void RefreshCameraForLoadedLevel()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var cameraController = mainCamera.GetComponent<MapCameraController>();
            if (cameraController != null)
            {
                cameraController.RefreshCamera();
            }
        }

        private void FindReferences()
        {
            if (levelDatabase == null)
            {
                levelDatabase = LevelDatabase.Instance;
            }

            if (battleController == null)
            {
                battleController = FindObjectOfType<BattleSceneController>();
            }

            if (loadingOverlay == null)
            {
                loadingOverlay = FindObjectOfType<LoadingOverlay>();
            }
        }

        private static void NotifyLevelSceneWillUnload(Scene scene)
        {
            foreach (var behaviour in FindObjectsOfType<MonoBehaviour>())
            {
                if (behaviour is ILevelSceneReferenceClient client)
                {
                    client.OnLevelSceneWillUnload(scene);
                }
            }
        }

        private static void NotifyLevelSceneLoaded(Scene scene)
        {
            foreach (var behaviour in FindObjectsOfType<MonoBehaviour>())
            {
                if (behaviour is ILevelSceneReferenceClient client)
                {
                    client.OnLevelSceneLoaded(scene);
                }
            }
        }
    }
}
