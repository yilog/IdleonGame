#if UNITY_EDITOR
using IdleonGame.Navigation;
using IdleonGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Editor
{
    public static class AddAutoNavigationToTestScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";

        [MenuItem("IdleonGame/Setup/Add Auto Navigation To Test Scene")]
        public static void AddAutoNavigation()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var pathfinderObject = GameObject.Find("TilemapNavigation");
            if (pathfinderObject == null)
            {
                pathfinderObject = new GameObject("TilemapNavigation");
            }

            if (pathfinderObject.GetComponent<TilemapNavigationPathfinder>() == null)
            {
                pathfinderObject.AddComponent<TilemapNavigationPathfinder>();
            }

            var player = GameObject.Find("Player_TestBlock");
            if (player != null && player.GetComponent<PlayerAutoNavigator>() == null)
            {
                player.AddComponent<PlayerAutoNavigator>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
