using System.Collections.Generic;
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

        private readonly List<GameObject> spawnedNodes = new();

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
            ClearNodes();
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

            var index = 0;
            foreach (var level in levelDatabase.Levels)
            {
                if (level == null)
                {
                    continue;
                }

                CreateNode(level, index++, runtimeData);
            }
        }

        private void CreateNode(LevelDefinition level, int index, PlayerRuntimeDataService runtimeData)
        {
            var nodeObject = new GameObject($"{level.LevelId}_Node", typeof(RectTransform));
            nodeObject.transform.SetParent(nodeRoot, false);
            spawnedNodes.Add(nodeObject);

            var rect = nodeObject.GetComponent<RectTransform>();
            var column = index % 3;
            var row = index / 3;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(118f, 76f);
            rect.anchoredPosition = new Vector2(120f + column * 170f, -92f - row * 118f);

            var isCurrent = runtimeData.Data.currentLevelId == level.LevelId;
            var isUnlocked = runtimeData.IsLevelUnlocked(level);
            var image = nodeObject.AddComponent<Image>();
            image.sprite = isCurrent ? currentNodeSprite : isUnlocked ? unlockedNodeSprite : lockedNodeSprite;
            image.color = isUnlocked ? Color.white : new Color32(120, 120, 120, 255);
            image.raycastTarget = true;

            var button = nodeObject.AddComponent<Button>();
            button.interactable = isUnlocked;
            if (isUnlocked)
            {
                button.onClick.AddListener(() => OnLevelClicked(level.LevelId));
            }

            var label = CreateText("Label", nodeObject.transform, level.DisplayName, 16, TextAnchor.MiddleCenter);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 6f);
            labelRect.offsetMax = new Vector2(-6f, -6f);
            label.color = isUnlocked ? Color.white : new Color32(210, 210, 210, 255);

            if (isCurrent && playerMarkerSprite != null)
            {
                var markerObject = new GameObject("PlayerMarker", typeof(RectTransform));
                markerObject.transform.SetParent(nodeObject.transform, false);
                var marker = markerObject.AddComponent<Image>();
                marker.sprite = playerMarkerSprite;
                marker.raycastTarget = false;

                var markerRect = markerObject.GetComponent<RectTransform>();
                markerRect.anchorMin = new Vector2(0.5f, 1f);
                markerRect.anchorMax = new Vector2(0.5f, 1f);
                markerRect.pivot = new Vector2(0.5f, 0f);
                markerRect.sizeDelta = new Vector2(28f, 28f);
                markerRect.anchoredPosition = new Vector2(0f, 6f);
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

        private void ClearNodes()
        {
            foreach (var node in spawnedNodes)
            {
                if (node != null)
                {
                    Destroy(node);
                }
            }

            spawnedNodes.Clear();
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var label = textObject.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }
    }
}
