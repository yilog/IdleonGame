using IdleonGame.Combat;
using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Player
{
    [CreateAssetMenu(fileName = "PlayerClassSkillDefinition", menuName = "IdleonGame/Player Class Skill Definition")]
    public sealed class PlayerClassSkillDefinition : ScriptableObject
    {
        [SerializeField] private PlayerClassType classType = PlayerClassType.None;
        [SerializeField] private AttackDefinition basicAttack;
        [SerializeField] private AttackDefinition secondaryAttack;

        public PlayerClassType ClassType => classType;
        public AttackDefinition BasicAttack => basicAttack;
        public AttackDefinition SecondaryAttack => secondaryAttack;

#if UNITY_EDITOR
        public void EditorSetData(PlayerClassType type, AttackDefinition basic, AttackDefinition secondary)
        {
            classType = type;
            basicAttack = basic;
            secondaryAttack = secondary;
        }
#endif
    }
}
