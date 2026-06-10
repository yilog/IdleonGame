using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Monster
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tilemap))]
    public sealed class MonsterSpawnTilemap : MonoBehaviour
    {
        [SerializeField] private Tilemap spawnTilemap;
        [SerializeField] private Tilemap groundTilemap;

        private readonly List<RuntimeSpawnPoint> spawnPoints = new();

        private void Awake()
        {
            if (spawnTilemap == null)
            {
                spawnTilemap = GetComponent<Tilemap>();
            }

            var tilemapRenderer = GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null)
            {
                tilemapRenderer.enabled = false;
            }

            if (groundTilemap == null)
            {
                var groundObject = GameObject.Find("Tilemap_Ground");
                groundTilemap = groundObject != null ? groundObject.GetComponent<Tilemap>() : null;
            }

            ScanSpawnTiles();
        }

        private void Update()
        {
            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.Tick(groundTilemap);
            }
        }

        public void Configure(Tilemap ground)
        {
            spawnTilemap = GetComponent<Tilemap>();
            groundTilemap = ground;
        }

        private void ScanSpawnTiles()
        {
            spawnPoints.Clear();
            if (spawnTilemap == null)
            {
                return;
            }

            spawnTilemap.CompressBounds();
            foreach (var cell in spawnTilemap.cellBounds.allPositionsWithin)
            {
                var spawnTile = spawnTilemap.GetTile<MonsterSpawnTile>(cell);
                if (spawnTile == null)
                {
                    continue;
                }

                spawnPoints.Add(new RuntimeSpawnPoint(spawnTile, spawnTilemap.GetCellCenterWorld(cell)));
            }
        }

        private sealed class RuntimeSpawnPoint
        {
            private readonly MonsterSpawnTile tile;
            private readonly Vector3 origin;
            private readonly List<MonsterController> activeMonsters = new();
            private float nextSpawnTime;

            public RuntimeSpawnPoint(MonsterSpawnTile tile, Vector3 origin)
            {
                this.tile = tile;
                this.origin = origin;
                nextSpawnTime = Time.time + tile.MinSpawnInterval;
            }

            public void Tick(Tilemap groundTilemap)
            {
                PruneDeadMonsters();
                if (activeMonsters.Count >= tile.MaxMonsterCount || Time.time < nextSpawnTime)
                {
                    return;
                }

                Spawn(groundTilemap);
                nextSpawnTime = Time.time + tile.MinSpawnInterval;
            }

            private void Spawn(Tilemap groundTilemap)
            {
                var definition = MonsterDatabase.Find(tile.MonsterId);
                if (definition == null)
                {
                    Debug.LogWarning($"Monster spawn point tried to spawn unknown monster id: {tile.MonsterId}");
                    return;
                }

                var offsetX = Random.Range(-tile.RandomRangeX, tile.RandomRangeX);
                var monster = MonsterFactory.CreateMonster(definition, origin + new Vector3(offsetX, 0f, 0f), groundTilemap);
                if (monster != null)
                {
                    activeMonsters.Add(monster);
                }
            }

            private void PruneDeadMonsters()
            {
                for (var i = activeMonsters.Count - 1; i >= 0; i--)
                {
                    if (activeMonsters[i] == null || activeMonsters[i].IsDead)
                    {
                        activeMonsters.RemoveAt(i);
                    }
                }
            }
        }
    }
}
