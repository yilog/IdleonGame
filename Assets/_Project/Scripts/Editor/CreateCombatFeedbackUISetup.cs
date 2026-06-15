#if UNITY_EDITOR
using System.IO;
using IdleonGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.Editor
{
    public static class CreateCombatFeedbackUISetup
    {
        private const string TextureFolder = "Assets/_Project/Art/UI/CombatFeedback";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/UI";
        private const string HealthBarPrefabPath = PrefabFolder + "/MonsterHealthBar.prefab";
        private const string DamageTextPrefabPath = PrefabFolder + "/DamageText.prefab";

        [MenuItem("IdleonGame/UI/Create Combat Feedback UI")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(PrefabFolder);

            var healthBackground = CreateBarSprite("MonsterHealthBarBack", new Color32(35, 20, 20, 230), new Color32(70, 55, 55, 255));
            var healthFill = CreateBarSprite("MonsterHealthBarFill", new Color32(196, 42, 54, 255), new Color32(255, 94, 108, 255));
            CreateHealthBarPrefab(healthBackground, healthFill);
            CreateDamageTextPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateHealthBarPrefab(Sprite backgroundSprite, Sprite fillSprite)
        {
            var root = File.Exists(HealthBarPrefabPath)
                ? PrefabUtility.LoadPrefabContents(HealthBarPrefabPath)
                : new GameObject("MonsterHealthBar", typeof(RectTransform), typeof(MonsterHealthBarView));

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(74f, 10f);

            var background = EnsureImage(root.transform, "Background", backgroundSprite);
            Stretch(background.rectTransform);
            background.type = Image.Type.Sliced;
            background.raycastTarget = false;

            var fill = EnsureImage(root.transform, "Fill", fillSprite);
            Stretch(fill.rectTransform, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;

            var serialized = new SerializedObject(root.GetComponent<MonsterHealthBarView>());
            serialized.FindProperty("fillImage").objectReferenceValue = fill;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, HealthBarPrefabPath);
        }

        private static void CreateDamageTextPrefab()
        {
            var root = File.Exists(DamageTextPrefabPath)
                ? PrefabUtility.LoadPrefabContents(DamageTextPrefabPath)
                : new GameObject("DamageText", typeof(RectTransform), typeof(DamageTextView));

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 34f);

            var text = root.GetComponent<Text>();
            if (text == null)
            {
                text = root.AddComponent<Text>();
            }

            text.text = "-0";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;

            var outline = root.GetComponent<Outline>();
            if (outline == null)
            {
                outline = root.AddComponent<Outline>();
            }

            outline.effectColor = new Color32(90, 0, 0, 255);
            outline.effectDistance = new Vector2(2f, -2f);

            var serialized = new SerializedObject(root.GetComponent<DamageTextView>());
            serialized.FindProperty("damageText").objectReferenceValue = text;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, DamageTextPrefabPath);
        }

        private static Image EnsureImage(Transform parent, string name, Sprite sprite)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
                obj.transform.SetParent(parent, false);
                child = obj.transform;
            }

            var image = child.GetComponent<Image>();
            if (image == null)
            {
                image = child.gameObject.AddComponent<Image>();
            }

            image.sprite = sprite;
            return image;
        }

        private static Sprite CreateBarSprite(string name, Color32 fill, Color32 highlight)
        {
            const int width = 96;
            const int height = 16;
            var path = $"{TextureFolder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var edge = x < 2 || y < 2 || x >= width - 2 || y >= height - 2;
                        texture.SetPixel(x, y, edge ? highlight : fill);
                    }
                }

                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void SavePrefab(GameObject root, string path)
        {
            var loadedExisting = File.Exists(path);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            if (loadedExisting)
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            else
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
#endif
