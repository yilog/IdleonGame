using UnityEngine.SceneManagement;

namespace IdleonGame.Levels
{
    public interface ILevelSceneReferenceClient
    {
        void OnLevelSceneWillUnload(Scene scene);

        void OnLevelSceneLoaded(Scene scene);
    }
}
