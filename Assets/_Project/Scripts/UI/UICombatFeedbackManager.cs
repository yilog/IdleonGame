using System.Collections.Generic;
using IdleonGame.Monster;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public sealed class UICombatFeedbackManager : MonoBehaviour
    {
        private const string HealthBarPrefabPath = "Prefabs/UI/MonsterHealthBar";
        private const string DamageTextPrefabPath = "Prefabs/UI/DamageText";
        private const float HealthBarLifetimeSeconds = 5f;
        private const int CombatFeedbackSortingOrder = -100;

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private MonsterHealthBarView healthBarPrefab;
        [SerializeField] private DamageTextView damageTextPrefab;

        private readonly Dictionary<MonsterController, MonsterHealthBarView> activeHealthBars = new();
        private readonly List<MonsterHealthBarView> healthBarPool = new();
        private readonly List<DamageTextView> activeDamageTexts = new();
        private readonly List<DamageTextView> damageTextPool = new();

        public static UICombatFeedbackManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureCanvas();
            LoadPrefabs();
        }

        private void Update()
        {
            TickHealthBars();
            TickDamageTexts();
        }

        public static UICombatFeedbackManager EnsureExists()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var owner = UIManager.EnsureExists();
            var managerObject = new GameObject("UICombatFeedbackManager");
            managerObject.transform.SetParent(owner.transform, false);
            return managerObject.AddComponent<UICombatFeedbackManager>();
        }

        public void ShowMonsterDamage(MonsterController monster, int damage)
        {
            if (monster == null || damage <= 0)
            {
                return;
            }

            EnsureCanvas();
            LoadPrefabs();
            ShowDamageText(monster, damage);

            if (monster.IsDead)
            {
                HideHealthBar(monster);
                return;
            }

            ShowOrRefreshHealthBar(monster);
        }

        public void HideHealthBar(MonsterController monster)
        {
            if (monster == null)
            {
                return;
            }

            if (activeHealthBars.TryGetValue(monster, out var view))
            {
                activeHealthBars.Remove(monster);
                ReleaseHealthBar(view);
            }
        }

        private void ShowOrRefreshHealthBar(MonsterController monster)
        {
            if (healthBarPrefab == null)
            {
                return;
            }

            if (!activeHealthBars.TryGetValue(monster, out var view))
            {
                view = GetHealthBar();
                activeHealthBars[monster] = view;
            }

            view.Show(monster, Camera.main, HealthBarLifetimeSeconds);
        }

        private void ShowDamageText(MonsterController monster, int damage)
        {
            if (damageTextPrefab == null || canvasRoot == null)
            {
                return;
            }

            var cameraToUse = Camera.main;
            if (cameraToUse == null)
            {
                return;
            }

            var screenPosition = cameraToUse.WorldToScreenPoint(monster.transform.position + new Vector3(0f, 1.25f, 0f));
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, screenPosition, null, out var localPoint))
            {
                return;
            }

            var view = GetDamageText();
            view.Show(damage, localPoint);
            activeDamageTexts.Add(view);
        }

        private void TickHealthBars()
        {
            if (activeHealthBars.Count == 0)
            {
                return;
            }

            var expired = new List<MonsterController>();
            foreach (var pair in activeHealthBars)
            {
                var view = pair.Value;
                if (view == null || view.IsExpired)
                {
                    expired.Add(pair.Key);
                    continue;
                }

                view.Tick();
            }

            foreach (var monster in expired)
            {
                if (activeHealthBars.TryGetValue(monster, out var view))
                {
                    activeHealthBars.Remove(monster);
                    ReleaseHealthBar(view);
                }
            }
        }

        private void TickDamageTexts()
        {
            for (var i = activeDamageTexts.Count - 1; i >= 0; i--)
            {
                var view = activeDamageTexts[i];
                if (view == null || view.IsExpired)
                {
                    activeDamageTexts.RemoveAt(i);
                    ReleaseDamageText(view);
                    continue;
                }

                view.Tick();
            }
        }

        private MonsterHealthBarView GetHealthBar()
        {
            if (healthBarPool.Count > 0)
            {
                var last = healthBarPool.Count - 1;
                var view = healthBarPool[last];
                healthBarPool.RemoveAt(last);
                return view;
            }

            var created = Instantiate(healthBarPrefab, canvasRoot);
            created.Initialize(canvasRoot);
            return created;
        }

        private DamageTextView GetDamageText()
        {
            if (damageTextPool.Count > 0)
            {
                var last = damageTextPool.Count - 1;
                var view = damageTextPool[last];
                damageTextPool.RemoveAt(last);
                return view;
            }

            return Instantiate(damageTextPrefab, canvasRoot);
        }

        private void ReleaseHealthBar(MonsterHealthBarView view)
        {
            if (view == null)
            {
                return;
            }

            view.Clear();
            view.transform.SetParent(canvasRoot, false);
            healthBarPool.Add(view);
        }

        private void ReleaseDamageText(DamageTextView view)
        {
            if (view == null)
            {
                return;
            }

            view.Clear();
            view.transform.SetParent(canvasRoot, false);
            damageTextPool.Add(view);
        }

        private void EnsureCanvas()
        {
            if (canvas != null && canvasRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("CombatFeedbackCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CombatFeedbackSortingOrder;
            canvasRoot = canvasObject.GetComponent<RectTransform>();
            canvasRoot.anchorMin = Vector2.zero;
            canvasRoot.anchorMax = Vector2.one;
            canvasRoot.offsetMin = Vector2.zero;
            canvasRoot.offsetMax = Vector2.zero;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void LoadPrefabs()
        {
            if (healthBarPrefab == null)
            {
                healthBarPrefab = Resources.Load<MonsterHealthBarView>(HealthBarPrefabPath);
            }

            if (damageTextPrefab == null)
            {
                damageTextPrefab = Resources.Load<DamageTextView>(DamageTextPrefabPath);
            }
        }
    }
}
