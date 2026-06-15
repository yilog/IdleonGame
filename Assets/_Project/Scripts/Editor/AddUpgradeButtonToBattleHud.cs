#if UNITY_EDITOR
using IdleonGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.Editor
{
    public static class AddUpgradeButtonToBattleHud
    {
        private const string PrefabPath = "Assets/_Project/Resources/Prefabs/UI/BattleHUD.prefab";
        private const string ButtonSpritePath = "Assets/_Project/Art/UI/BattleHUD/HudButton.png";
        private const string UpgradeIconPath = "Assets/_Project/Art/UI/Upgrades/Upgrade_BiggerDamage.png";

        [MenuItem("IdleonGame/UI/Add Upgrade Button To Battle HUD")]
        public static void Apply()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"BattleHUD prefab not found: {PrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            var controller = root.GetComponent<BattleHudWindowController>();
            if (controller == null)
            {
                Debug.LogError("BattleHUD prefab does not contain BattleHudWindowController.");
                PrefabUtility.UnloadPrefabContents(root);
                return;
            }

            var existing = root.transform.Find("UpgradeButton");
            var button = existing != null ? existing.GetComponent<Button>() : CreateUpgradeButton(root.transform);

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("upgradeButton").objectReferenceValue = button;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Button CreateUpgradeButton(Transform parent)
        {
            var buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            var iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UpgradeIconPath);

            var obj = new GameObject("UpgradeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(12f, -12f);
            rect.sizeDelta = new Vector2(92f, 64f);

            var image = obj.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(obj.transform, false);
            var icon = iconObject.GetComponent<Image>();
            icon.sprite = iconSprite;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -20f);
            iconRect.sizeDelta = new Vector2(34f, 30f);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(obj.transform, false);
            var label = labelObject.GetComponent<Text>();
            label.text = "UPGRADE";
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 12;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 13f);
            labelRect.sizeDelta = new Vector2(0f, 22f);

            return obj.GetComponent<Button>();
        }
    }
}
#endif
