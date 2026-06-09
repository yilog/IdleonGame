using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Map
{
    [DisallowMultipleComponent]
    public sealed class MapTilemapLayer : MonoBehaviour
    {
        [SerializeField] private TilemapLayerType layerType;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TilemapRenderer tilemapRenderer;
        [SerializeField] private TilemapCollider2D tilemapCollider;

        public TilemapLayerType LayerType => layerType;
        public Tilemap Tilemap => tilemap;
        public TilemapRenderer TilemapRenderer => tilemapRenderer;
        public TilemapCollider2D TilemapCollider => tilemapCollider;

        public void Configure(TilemapLayerType type, int sortingOrder, bool hasCollider)
        {
            layerType = type;
            tilemap = GetComponent<Tilemap>();
            tilemapRenderer = GetComponent<TilemapRenderer>();
            tilemapCollider = GetComponent<TilemapCollider2D>();

            if (tilemapRenderer != null)
            {
                tilemapRenderer.sortingOrder = sortingOrder;
            }

            if (hasCollider && tilemapCollider == null)
            {
                tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
            }
            else if (!hasCollider && tilemapCollider != null)
            {
                tilemapCollider.enabled = false;
            }
        }

        private void Reset()
        {
            tilemap = GetComponent<Tilemap>();
            tilemapRenderer = GetComponent<TilemapRenderer>();
            tilemapCollider = GetComponent<TilemapCollider2D>();
        }

        private void OnValidate()
        {
            if (tilemap == null)
            {
                tilemap = GetComponent<Tilemap>();
            }

            if (tilemapRenderer == null)
            {
                tilemapRenderer = GetComponent<TilemapRenderer>();
            }

            if (tilemapCollider == null)
            {
                tilemapCollider = GetComponent<TilemapCollider2D>();
            }
        }
    }
}