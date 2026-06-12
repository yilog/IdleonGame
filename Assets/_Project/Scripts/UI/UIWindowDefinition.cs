using System;
using UnityEngine;

namespace IdleonGame.UI
{
    [Serializable]
    public sealed class UIWindowDefinition
    {
        [SerializeField] private string windowId;
        [SerializeField] private GameObject prefab;
        [SerializeField] private string resourcesPath;
        [SerializeField] private UIWindowMode mode = UIWindowMode.Floating;
        [SerializeField] private bool modal;
        [SerializeField] private bool openOnStart;

        public string WindowId => windowId;
        public GameObject Prefab => prefab;
        public string ResourcesPath => resourcesPath;
        public UIWindowMode Mode => mode;
        public bool Modal => modal;
        public bool OpenOnStart => openOnStart;

        public UIWindowDefinition()
        {
        }

        public UIWindowDefinition(string id, GameObject windowPrefab, string prefabResourcesPath, UIWindowMode windowMode, bool isModal, bool shouldOpenOnStart)
        {
            windowId = id;
            prefab = windowPrefab;
            resourcesPath = prefabResourcesPath;
            mode = windowMode;
            modal = isModal;
            openOnStart = shouldOpenOnStart;
        }
    }
}
