#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IdleonGame.Editor
{
    [InitializeOnLoad]
    public static class ProjectRenderingSetup
    {
        private const string RenderingFolder = "Assets/_Project/Settings/Rendering";
        private const string PipelineAssetPath = RenderingFolder + "/IdleonGame_URP_2D.asset";
        private const string RendererAssetPath = RenderingFolder + "/IdleonGame_2DRenderer.asset";
        private const string SessionKey = "IdleonGame.Create2DURPAssets.Pending";

        static ProjectRenderingSetup()
        {
            EditorApplication.delayCall += RunPendingSetup;
        }

        [MenuItem("IdleonGame/Setup/Create 2D URP Rendering Assets")]
        public static void Create2DURPAssets()
        {
            CreateAndAssignAssets();
        }

        [MenuItem("IdleonGame/Setup/Queue 2D URP Rendering Setup")]
        public static void Queue2DURPAssetsSetup()
        {
            SessionState.SetBool(SessionKey, true);
            CreateAndAssignAssets();
        }

        private static void RunPendingSetup()
        {
            if (!SessionState.GetBool(SessionKey, true))
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunPendingSetup;
                return;
            }

            CreateAndAssignAssets();
            SessionState.SetBool(SessionKey, false);
        }

        private static void CreateAndAssignAssets()
        {
            Directory.CreateDirectory(RenderingFolder);

            var renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererAssetPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, RendererAssetPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            }

            GraphicsSettings.renderPipelineAsset = pipeline;
            QualitySettings.renderPipeline = pipeline;

            EditorUtility.SetDirty(pipeline);
            EditorUtility.SetDirty(renderer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif