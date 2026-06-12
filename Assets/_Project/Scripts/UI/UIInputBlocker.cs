using UnityEngine.EventSystems;

namespace IdleonGame.UI
{
    public static class UIInputBlocker
    {
        public static bool ShouldBlockScenePointerInput()
        {
            var manager = UIManager.Instance;
            if (manager != null && manager.HasModalWindow)
            {
                return true;
            }

            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
