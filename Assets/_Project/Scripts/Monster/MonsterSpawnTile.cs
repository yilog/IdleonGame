using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Monster
{
    [CreateAssetMenu(fileName = "MonsterSpawnTile", menuName = "IdleonGame/Tilemap/Monster Spawn Tile")]
    public sealed class MonsterSpawnTile : Tile
    {
        [SerializeField] private string monsterId = "test_walker";
        [SerializeField] private float minSpawnInterval = 5f;
        [SerializeField] private int maxMonsterCount = 10;
        [SerializeField] private float randomRangeX = 0.5f;

        public string MonsterId => monsterId;
        public float MinSpawnInterval => Mathf.Max(0.1f, minSpawnInterval);
        public int MaxMonsterCount => Mathf.Max(1, maxMonsterCount);
        public float RandomRangeX => Mathf.Max(0f, randomRangeX);

#if UNITY_EDITOR
        public void EditorSetData(string spawnMonsterId, float interval, int maxCount, float rangeX, Sprite tileSprite)
        {
            monsterId = spawnMonsterId;
            minSpawnInterval = Mathf.Max(0.1f, interval);
            maxMonsterCount = Mathf.Max(1, maxCount);
            randomRangeX = Mathf.Max(0f, rangeX);
            sprite = tileSprite;
            colliderType = Tile.ColliderType.None;
        }
#endif
    }
}
