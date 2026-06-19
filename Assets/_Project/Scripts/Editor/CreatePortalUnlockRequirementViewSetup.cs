#if UNITY_EDITOR
using System.IO;
using IdleonGame.Core;
using IdleonGame.Map;
using UnityEditor;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreatePortalUnlockRequirementViewSetup
    {
        private const string ArtFolder = "Assets/_Project/Art/Map";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/Map";
        private const string SkullTexturePath = ArtFolder + "/PortalRequirementSkull.png";
        private const string PrefabPath = PrefabFolder + "/PortalUnlockRequirementView.prefab";

        [MenuItem("IdleonGame/Setup/Create Portal Unlock Requirement View")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(ArtFolder);
            Directory.CreateDirectory(PrefabFolder);

            var skullSprite = CreateSkullSprite();
            CreatePrefab(skullSprite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created portal unlock requirement view prefab: {PrefabPath}");
        }

        private static Sprite CreateSkullSprite()
        {
            if (!File.Exists(SkullTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                var clear = new Color32(0, 0, 0, 0);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        texture.SetPixel(x, y, clear);
                    }
                }

                DrawFilledCircle(texture, 8, 9, 6, new Color32(238, 238, 226, 255));
                DrawRect(texture, 5, 3, 6, 4, new Color32(238, 238, 226, 255));
                DrawFilledCircle(texture, 6, 10, 2, new Color32(42, 42, 46, 255));
                DrawFilledCircle(texture, 10, 10, 2, new Color32(42, 42, 46, 255));
                DrawRect(texture, 7, 7, 2, 2, new Color32(42, 42, 46, 255));
                DrawRect(texture, 5, 3, 1, 2, new Color32(42, 42, 46, 255));
                DrawRect(texture, 7, 3, 1, 2, new Color32(42, 42, 46, 255));
                DrawRect(texture, 9, 3, 1, 2, new Color32(42, 42, 46, 255));
                texture.Apply();

                File.WriteAllBytes(SkullTexturePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(SkullTexturePath);
            }

            var importer = AssetImporter.GetAtPath(SkullTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SkullTexturePath);
        }

        private static void CreatePrefab(Sprite skullSprite)
        {
            var root = new GameObject("PortalUnlockRequirementView", typeof(MapPortalUnlockRequirementPrefabView));

            var skullObject = new GameObject("SkullIcon", typeof(SpriteRenderer));
            skullObject.transform.SetParent(root.transform, false);
            skullObject.transform.localPosition = new Vector3(-0.18f, 0f, 0f);
            var skullRenderer = skullObject.GetComponent<SpriteRenderer>();
            skullRenderer.sprite = skullSprite;
            skullRenderer.sortingOrder = GameRenderLayers.SortingOrders.PortalRequirement;

            var textObject = new GameObject("KillCount", typeof(TextMesh));
            textObject.transform.SetParent(root.transform, false);
            textObject.transform.localPosition = new Vector3(0.15f, -0.02f, 0f);
            var countText = textObject.GetComponent<TextMesh>();
            countText.text = "0";
            countText.anchor = TextAnchor.MiddleLeft;
            countText.alignment = TextAlignment.Left;
            countText.characterSize = 0.22f;
            countText.color = Color.red;
            countText.fontSize = 20;
            var textRenderer = textObject.GetComponent<MeshRenderer>();
            textRenderer.sortingOrder = GameRenderLayers.SortingOrders.PortalRequirement + 1;

            var prefabView = root.GetComponent<MapPortalUnlockRequirementPrefabView>();
            var serialized = new SerializedObject(prefabView);
            serialized.FindProperty("skullIcon").objectReferenceValue = skullRenderer;
            serialized.FindProperty("countText").objectReferenceValue = countText;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void DrawFilledCircle(Texture2D texture, int centerX, int centerY, int radius, Color32 color)
        {
            var radiusSquared = radius * radius;
            for (var y = centerY - radius; y <= centerY + radius; y++)
            {
                for (var x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    var dx = x - centerX;
                    var dy = y - centerY;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color32 color)
        {
            for (var py = y; py < y + height; py++)
            {
                for (var px = x; px < x + width; px++)
                {
                    if (px >= 0 && py >= 0 && px < texture.width && py < texture.height)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }
    }
}
#endif
