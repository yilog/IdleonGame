using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Player
{
    [CreateAssetMenu(fileName = "PlayerClassPresentationDefinition", menuName = "IdleonGame/Player Class Presentation Definition")]
    public sealed class PlayerClassPresentationDefinition : ScriptableObject
    {
        [SerializeField] private PlayerClassType classType = PlayerClassType.None;
        [SerializeField] private string displayName = "No Class";
        [SerializeField] private GameObject prefab;
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private Sprite previewSprite;

        public PlayerClassType ClassType => classType;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public Sprite PreviewSprite => previewSprite;

#if UNITY_EDITOR
        public void EditorSetData(
            PlayerClassType type,
            string classDisplayName,
            GameObject classPrefab,
            RuntimeAnimatorController controller,
            Sprite sprite)
        {
            classType = type;
            displayName = classDisplayName;
            prefab = classPrefab;
            animatorController = controller;
            previewSprite = sprite;
        }
#endif
    }
}
