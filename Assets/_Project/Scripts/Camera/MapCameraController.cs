using Cinemachine;
using IdleonGame.Levels;
using IdleonGame.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Cameras
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class MapCameraController : MonoBehaviour, ILevelSceneReferenceClient
    {
        private const string PlayerObjectName = "Player_TestBlock";
        private const string VirtualCameraName = "CM_PlayerFollowCamera";
        private const string BoundsObjectName = "CM_MapCameraBounds";

        [SerializeField] private Transform player;
        [SerializeField] private BattleMapController mapController;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineConfiner2D confiner;

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        //private static void Bootstrap()
        //{
        //    var mainCamera = UnityEngine.Camera.main;
        //    if (mainCamera != null && mainCamera.GetComponent<MapCameraController>() == null)
        //    {
        //        mainCamera.gameObject.AddComponent<MapCameraController>();
        //    }
        //}

        private void Start()
        {
            ConfigureCamera();
        }

        public void RefreshCamera()
        {
            player = null;
            mapController = null;
            ConfigureCamera();
        }

        public void OnLevelSceneWillUnload(Scene scene)
        {
            if (mapController != null && scene.IsValid() && mapController.gameObject.scene == scene)
            {
                mapController = null;
            }
        }

        public void OnLevelSceneLoaded(Scene scene)
        {
            if (scene.IsValid())
            {
                RefreshCamera();
            }
        }

        private void ConfigureCamera()
        {
            FindSceneReferences();

            var mainCamera = GetComponent<UnityEngine.Camera>();
            var brain = GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = gameObject.AddComponent<CinemachineBrain>();
            }

            if (virtualCamera == null)
            {
                virtualCamera = FindOrCreateVirtualCamera(mainCamera);
            }

            virtualCamera.Follow = player;
            virtualCamera.Priority = 10;
            virtualCamera.m_Lens.Orthographic = true;
            virtualCamera.m_Lens.OrthographicSize = mainCamera.orthographicSize;
            virtualCamera.transform.position = new Vector3(
                mainCamera.transform.position.x,
                mainCamera.transform.position.y,
                mainCamera.transform.position.z);
            ConfigureFramingTransposer(mainCamera);

            var boundsCollider = FindOrCreateBoundsCollider();
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner == null)
            {
                confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
            }

            confiner.m_BoundingShape2D = boundsCollider;
            confiner.InvalidateCache();
        }

        private void ConfigureFramingTransposer(UnityEngine.Camera mainCamera)
        {
            var transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer == null)
            {
                transposer = virtualCamera.AddCinemachineComponent<CinemachineFramingTransposer>();
            }

            var targetZ = player != null ? player.position.z : 0f;
            transposer.m_CameraDistance = Mathf.Abs(targetZ - mainCamera.transform.position.z);
            transposer.m_TrackedObjectOffset = Vector3.zero;
            transposer.m_XDamping = 0f;
            transposer.m_YDamping = 0f;
            transposer.m_ZDamping = 0f;
        }

        private CinemachineVirtualCamera FindOrCreateVirtualCamera(UnityEngine.Camera mainCamera)
        {
            var existing = FindObjectOfType<CinemachineVirtualCamera>();
            if (existing != null)
            {
                return existing;
            }

            var cameraObject = new GameObject(VirtualCameraName);
            var created = cameraObject.AddComponent<CinemachineVirtualCamera>();
            created.m_Lens.Orthographic = true;
            created.m_Lens.OrthographicSize = mainCamera.orthographicSize;
            return created;
        }

        private PolygonCollider2D FindOrCreateBoundsCollider()
        {
            var boundsObject = GameObject.Find(BoundsObjectName);
            if (boundsObject == null)
            {
                boundsObject = new GameObject(BoundsObjectName);
            }

            var collider = boundsObject.GetComponent<PolygonCollider2D>();
            if (collider == null)
            {
                collider = boundsObject.AddComponent<PolygonCollider2D>();
            }

            collider.isTrigger = true;
            collider.pathCount = 1;
            collider.SetPath(0, CreateBoundsPath());
            return collider;
        }

        private Vector2[] CreateBoundsPath()
        {
            if (mapController == null)
            {
                return new[]
                {
                    new Vector2(-10f, -5f),
                    new Vector2(-10f, 5f),
                    new Vector2(10f, 5f),
                    new Vector2(10f, -5f)
                };
            }

            var bounds = mapController.TileBounds;
            var min = new Vector2(bounds.xMin, bounds.yMin);
            var max = new Vector2(bounds.xMax, bounds.yMax);
            return new[]
            {
                new Vector2(min.x, min.y),
                new Vector2(min.x, max.y),
                new Vector2(max.x, max.y),
                new Vector2(max.x, min.y)
            };
        }

        private void FindSceneReferences()
        {
            if (player == null)
            {
                var playerObject = GameObject.Find(PlayerObjectName);
                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
            }

            if (mapController == null)
            {
                mapController = LevelSceneReferenceResolver.FindInActiveScene<BattleMapController>();
            }
        }
    }
}
