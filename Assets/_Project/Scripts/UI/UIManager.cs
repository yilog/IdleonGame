using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        private const string DefaultBattleHudWindowId = "battle_hud";
        private const string DefaultTestWindowId = "test_window";
        private const string DefaultMapWindowId = UIMapWindowController.WindowIdConst;
        private const int FullscreenBaseOrder = 1000;
        private const int FloatingBaseOrder = 2000;

        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private RectTransform fullscreenRoot;
        [SerializeField] private RectTransform floatingRoot;
        [SerializeField] private Image modalBlocker;
        [SerializeField] private string battleHudWindowId = DefaultBattleHudWindowId;
        [SerializeField] private bool openBattleHudOnStart = true;
        [SerializeField] private bool enableTestWindowHotkey = true;
        [SerializeField] private KeyCode testWindowKey = KeyCode.U;
        [SerializeField] private bool enableMapWindowHotkey = true;
        [SerializeField] private KeyCode mapWindowKey = KeyCode.M;
        [SerializeField] private List<UIWindowDefinition> windows = new();

        private readonly Dictionary<string, UIWindowDefinition> definitions = new();
        private readonly Dictionary<string, UIWindowController> openWindows = new();
        private readonly List<UIWindowController> windowStack = new();
        private int nextFullscreenOrder = FullscreenBaseOrder;
        private int nextFloatingOrder = FloatingBaseOrder;

        public static UIManager Instance { get; private set; }
        public UIEventBus Events { get; } = new();
        public bool HasModalWindow => FindTopModalWindow() != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureInfrastructure();
            RebuildDefinitions();
        }

        private void Start()
        {
            OpenStartupWindows();
        }

        private void Update()
        {
            if (enableTestWindowHotkey && UnityEngine.Input.GetKeyDown(testWindowKey))
            {
                OpenWindow(DefaultTestWindowId);
            }

            if (enableMapWindowHotkey && UnityEngine.Input.GetKeyDown(mapWindowKey))
            {
                OpenWindow(DefaultMapWindowId);
            }
        }

        public static UIManager EnsureExists()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var managerObject = new GameObject("UIManager");
            return managerObject.AddComponent<UIManager>();
        }

        public T OpenWindow<T>(string windowId, object args = null) where T : UIWindowController
        {
            return OpenWindow(windowId, args) as T;
        }

        public UIWindowController OpenWindow(string windowId, object args = null)
        {
            EnsureInfrastructure();
            RebuildDefinitions();

            if (openWindows.TryGetValue(windowId, out var existing))
            {
                BringToFront(existing);
                return existing;
            }

            if (!definitions.TryGetValue(windowId, out var definition))
            {
                definition = CreateFallbackDefinition(windowId);
                definitions[windowId] = definition;
            }

            var windowObject = CreateWindowObject(definition);
            var controller = windowObject.GetComponent<UIWindowController>();
            if (controller == null)
            {
                controller = AddFallbackController(windowObject, definition);
            }

            var parent = definition.Mode == UIWindowMode.Fullscreen ? fullscreenRoot : floatingRoot;
            windowObject.transform.SetParent(parent, false);
            SetStretchIfRectTransform(windowObject.transform as RectTransform, definition.Mode);

            controller.Initialize(this, definition, args);
            openWindows[windowId] = controller;
            windowStack.Add(controller);
            BringToFront(controller);
            UpdateModalBlocker();
            return controller;
        }

        public void CloseWindow(string windowId)
        {
            if (!openWindows.TryGetValue(windowId, out var window))
            {
                return;
            }

            window.InternalClose();
            openWindows.Remove(windowId);
            windowStack.Remove(window);
            Destroy(window.gameObject);
            UpdateModalBlocker();
        }

        public T GetWindow<T>(string windowId) where T : UIWindowController
        {
            return openWindows.TryGetValue(windowId, out var window) ? window as T : null;
        }

        public bool TryGetWindow<T>(string windowId, out T window) where T : UIWindowController
        {
            window = GetWindow<T>(windowId);
            return window != null;
        }

        public void BringToFront(UIWindowController window)
        {
            if (window == null)
            {
                return;
            }

            windowStack.Remove(window);
            windowStack.Add(window);

            var order = window.Mode == UIWindowMode.Fullscreen ? ++nextFullscreenOrder : ++nextFloatingOrder;
            window.SetSortingOrder(order);
            UpdateModalBlocker();
        }

        public void RegisterDefinition(UIWindowDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.WindowId))
            {
                return;
            }

            definitions[definition.WindowId] = definition;
        }

        private void RebuildDefinitions()
        {
            foreach (var definition in windows)
            {
                RegisterDefinition(definition);
            }

            if (!definitions.ContainsKey(DefaultBattleHudWindowId))
            {
                definitions[DefaultBattleHudWindowId] = new UIWindowDefinition(DefaultBattleHudWindowId, null, "Prefabs/UI/BattleHUD", UIWindowMode.Fullscreen, false, true);
            }

            if (!definitions.ContainsKey(DefaultTestWindowId))
            {
                definitions[DefaultTestWindowId] = new UIWindowDefinition(DefaultTestWindowId, null, null, UIWindowMode.Floating, true, false);
            }

            if (!definitions.ContainsKey(DefaultMapWindowId))
            {
                definitions[DefaultMapWindowId] = new UIWindowDefinition(DefaultMapWindowId, null, "Prefabs/UI/UIMap", UIWindowMode.Floating, true, false);
            }
        }

        private void OpenStartupWindows()
        {
            if (openBattleHudOnStart && !string.IsNullOrEmpty(battleHudWindowId))
            {
                OpenWindow(battleHudWindowId);
            }

            foreach (var definition in windows)
            {
                if (definition != null && definition.OpenOnStart)
                {
                    OpenWindow(definition.WindowId);
                }
            }
        }

        private UIWindowDefinition CreateFallbackDefinition(string windowId)
        {
            if (windowId == DefaultBattleHudWindowId)
            {
                return new UIWindowDefinition(DefaultBattleHudWindowId, null, "Prefabs/UI/BattleHUD", UIWindowMode.Fullscreen, false, true);
            }

            return new UIWindowDefinition(windowId, null, null, UIWindowMode.Floating, true, false);
        }

        private GameObject CreateWindowObject(UIWindowDefinition definition)
        {
            var prefab = definition.Prefab;
            if (prefab == null && !string.IsNullOrEmpty(definition.ResourcesPath))
            {
                prefab = Resources.Load<GameObject>(definition.ResourcesPath);
            }

            return prefab != null ? Instantiate(prefab) : CreateFallbackWindowObject(definition);
        }

        private static GameObject CreateFallbackWindowObject(UIWindowDefinition definition)
        {
            var windowObject = new GameObject($"{definition.WindowId}_Window", typeof(RectTransform));
            if (definition.WindowId == DefaultBattleHudWindowId)
            {
                windowObject.AddComponent<BattleHudWindowController>();
                return windowObject;
            }

            if (definition.WindowId == DefaultTestWindowId)
            {
                windowObject.AddComponent<TestCloseWindowController>();
                return windowObject;
            }

            if (definition.WindowId == DefaultMapWindowId)
            {
                windowObject.AddComponent<UIMapWindowController>();
                return windowObject;
            }

            windowObject.AddComponent<GenericWindowController>();
            return windowObject;
        }

        private static UIWindowController AddFallbackController(GameObject windowObject, UIWindowDefinition definition)
        {
            if (definition.WindowId == DefaultBattleHudWindowId)
            {
                return windowObject.AddComponent<BattleHudWindowController>();
            }

            if (definition.WindowId == DefaultTestWindowId)
            {
                return windowObject.AddComponent<TestCloseWindowController>();
            }

            if (definition.WindowId == DefaultMapWindowId)
            {
                return windowObject.AddComponent<UIMapWindowController>();
            }

            return windowObject.AddComponent<GenericWindowController>();
        }

        private void EnsureInfrastructure()
        {
            EnsureEventSystem();
            EnsureCanvas();
            fullscreenRoot = EnsureLayer("FullscreenRoot", fullscreenRoot);
            floatingRoot = EnsureLayer("FloatingRoot", floatingRoot);
            EnsureModalBlocker();
        }

        private void EnsureCanvas()
        {
            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInChildren<Canvas>();
            }

            if (rootCanvas == null)
            {
                var canvasObject = new GameObject("UIRootCanvas", typeof(RectTransform));
                canvasObject.transform.SetParent(transform, false);
                rootCanvas = canvasObject.AddComponent<Canvas>();
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 0;

            var scaler = rootCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        private RectTransform EnsureLayer(string layerName, RectTransform current)
        {
            if (current == null)
            {
                var existing = rootCanvas.transform.Find(layerName);
                current = existing as RectTransform;
            }

            if (current == null)
            {
                var layerObject = new GameObject(layerName, typeof(RectTransform));
                layerObject.transform.SetParent(rootCanvas.transform, false);
                current = layerObject.GetComponent<RectTransform>();
            }

            SetStretchIfRectTransform(current, UIWindowMode.Fullscreen);
            return current;
        }

        private void EnsureModalBlocker()
        {
            if (modalBlocker == null)
            {
                var blockerObject = new GameObject("ModalBlocker", typeof(RectTransform));
                blockerObject.transform.SetParent(rootCanvas.transform, false);
                modalBlocker = blockerObject.AddComponent<Image>();
                blockerObject.AddComponent<Canvas>();
                blockerObject.AddComponent<GraphicRaycaster>();
            }

            modalBlocker.color = new Color(0f, 0f, 0f, 0.35f);
            modalBlocker.raycastTarget = true;
            SetStretchIfRectTransform(modalBlocker.rectTransform, UIWindowMode.Fullscreen);
            modalBlocker.gameObject.SetActive(false);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystemObject);
        }

        private void UpdateModalBlocker()
        {
            if (modalBlocker == null)
            {
                return;
            }

            var topModal = FindTopModalWindow();
            modalBlocker.gameObject.SetActive(topModal != null);
            if (topModal == null)
            {
                return;
            }

            var blockerCanvas = modalBlocker.GetComponent<Canvas>();
            if (blockerCanvas != null)
            {
                blockerCanvas.overrideSorting = true;
                blockerCanvas.sortingOrder = topModal.SortingOrder - 1;
            }
        }

        private UIWindowController FindTopModalWindow()
        {
            for (var i = windowStack.Count - 1; i >= 0; i--)
            {
                var window = windowStack[i];
                if (window != null && window.IsModal)
                {
                    return window;
                }
            }

            return null;
        }

        private static void SetStretchIfRectTransform(RectTransform rectTransform, UIWindowMode mode)
        {
            if (rectTransform == null)
            {
                return;
            }

            if (mode == UIWindowMode.Fullscreen)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                return;
            }

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = rectTransform.sizeDelta == Vector2.zero ? new Vector2(420f, 240f) : rectTransform.sizeDelta;
        }
    }
}
