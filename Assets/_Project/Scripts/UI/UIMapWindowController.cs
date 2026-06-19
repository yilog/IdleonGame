using IdleonGame.Data;
using IdleonGame.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class UIMapWindowController : UIWindowController
    {
        public const string WindowIdConst = "ui_map";

        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Sprite unlockedNodeSprite;
        [SerializeField] private Sprite lockedNodeSprite;
        [SerializeField] private Sprite currentNodeSprite;
        [SerializeField] private Sprite playerMarkerSprite;
        [SerializeField] private string[] fixedLevelIds =
        {
            "level1_1",
            "level1_2",
            "level1_3",
            "level2_1",
            "level2_2",
            "level2_3"
        };

        private static readonly string[] DefaultFixedLevelIds =
        {
            "level1_1",
            "level1_2",
            "level1_3",
            "level2_1",
            "level2_2",
            "level2_3"
        };

        protected override void OnOpen(object args)
        {
            EnsureReferences();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }

            Refresh();
        }

        protected override void OnClose()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }

        public void Refresh()
        {
            var runtimeData = PlayerRuntimeDataService.EnsureExists();
            var levelDatabase = LevelDatabase.Instance;
            runtimeData.RefreshLevelUnlocks(levelDatabase);

            if (titleText != null)
            {
                titleText.text = $"Current Map: {runtimeData.Data.currentLevelId}";
            }

            if (levelDatabase == null || nodeRoot == null)
            {
                return;
            }

            var levelIds = fixedLevelIds != null && fixedLevelIds.Length > 0 ? fixedLevelIds : DefaultFixedLevelIds;
            foreach (var levelId in levelIds)
            {
                if (string.IsNullOrEmpty(levelId))
                {
                    continue;
                }

                RefreshNode(levelId, levelDatabase, runtimeData);
            }
        }

        private void RefreshNode(string levelId, LevelDatabase levelDatabase, PlayerRuntimeDataService runtimeData)
        {
            var node = FindNode(levelId);
            if (node == null)
            {
                Debug.LogWarning($"UIMap missing fixed node for level id: {levelId}");
                return;
            }

            var level = levelDatabase.GetLevel(levelId);
            node.gameObject.SetActive(level != null);
            if (level == null)
            {
                return;
            }

            var isCurrent = runtimeData.Data.currentLevelId == level.LevelId;
            var isUnlocked = runtimeData.IsLevelUnlocked(level);
            var image = node.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = isCurrent ? currentNodeSprite : isUnlocked ? unlockedNodeSprite : lockedNodeSprite;
                image.color = isUnlocked ? Color.white : new Color32(120, 120, 120, 255);
                image.raycastTarget = true;
            }

            var button = node.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.interactable = isUnlocked;
                if (isUnlocked)
                {
                    button.onClick.AddListener(() => OnLevelClicked(level.LevelId));
                }
            }

            var labelTransform = node.Find("Label");
            var label = labelTransform != null ? labelTransform.GetComponent<Text>() : null;
            if (label != null)
            {
                label.text = level.DisplayName;
                label.color = isUnlocked ? Color.white : new Color32(210, 210, 210, 255);
            }

            var markerTransform = node.Find("PlayerMarker");
            if (markerTransform != null)
            {
                markerTransform.gameObject.SetActive(isCurrent);
                var markerImage = markerTransform.GetComponent<Image>();
                if (markerImage != null && playerMarkerSprite != null)
                {
                    markerImage.sprite = playerMarkerSprite;
                }
            }
        }

        private void OnLevelClicked(string levelId)
        {
            var transitionService = LevelSceneTransitionService.Instance;
            if (transitionService == null)
            {
                Debug.LogWarning("UIMap cannot find LevelSceneTransitionService.");
                return;
            }

            Close();
            transitionService.SwitchToLevel(levelId);
        }

        private void EnsureReferences()
        {
            if (nodeRoot == null)
            {
                var found = transform.Find("Body/NodeRoot");
                nodeRoot = found as RectTransform;
            }

            if (closeButton == null)
            {
                closeButton = GetComponentInChildren<Button>();
            }

            if (titleText == null)
            {
                var found = transform.Find("Header/Title");
                titleText = found != null ? found.GetComponent<Text>() : null;
            }
        }

        private RectTransform FindNode(string levelId)
        {
            var direct = nodeRoot.Find($"{levelId}_Node") as RectTransform;
            if (direct != null)
            {
                return direct;
            }

            var fallback = nodeRoot.Find(levelId) as RectTransform;
            return fallback;
        }
    }
}
