using UnityEngine;

namespace IdleonGame.Character
{
    public static class CharacterAnchor2D
    {
        public static readonly Vector2 BottomCenterPivot = new(0.5f, 0f);

        public static readonly Vector2 PlayerColliderSize = new(0.9f, 0.95f);
        public static readonly Vector2 PlayerColliderOffset = new(0f, PlayerColliderSize.y * 0.5f);

        public static readonly Vector2 MonsterColliderSize = new(0.8f, 0.9f);
        public static readonly Vector2 MonsterColliderOffset = new(0f, MonsterColliderSize.y * 0.5f);
    }
}
