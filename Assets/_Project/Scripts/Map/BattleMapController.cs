using System.Collections.Generic;
using UnityEngine;

namespace IdleonGame.Map
{
    [DisallowMultipleComponent]
    public sealed class BattleMapController : MonoBehaviour
    {
        [SerializeField] private MapSceneDefinition mapDefinition;
        [SerializeField] private BoundsInt tileBounds;
        [SerializeField] private List<MapTilemapLayer> tilemapLayers = new();

        private readonly Dictionary<TilemapLayerType, MapTilemapLayer> layersByType = new();

        public MapSceneDefinition MapDefinition => mapDefinition;
        public BoundsInt TileBounds => tileBounds;

        public bool TryGetLayer(TilemapLayerType layerType, out MapTilemapLayer layer)
        {
            EnsureLayerLookup();
            return layersByType.TryGetValue(layerType, out layer);
        }

#if UNITY_EDITOR
        public void EditorConfigure(MapSceneDefinition definition, BoundsInt bounds, List<MapTilemapLayer> layers)
        {
            mapDefinition = definition;
            tileBounds = bounds;
            tilemapLayers = layers;
            EnsureLayerLookup();
        }
#endif

        private void Awake()
        {
            EnsureLayerLookup();
        }

        private void EnsureLayerLookup()
        {
            layersByType.Clear();

            foreach (var layer in tilemapLayers)
            {
                if (layer == null)
                {
                    continue;
                }

                layersByType[layer.LayerType] = layer;
            }
        }
    }
}