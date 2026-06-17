using System;
using System.Collections.Generic;
using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Player
{
    [CreateAssetMenu(fileName = "PlayerClassPresentationDatabase", menuName = "IdleonGame/Player Class Presentation Database")]
    public sealed class PlayerClassPresentationDatabase : ScriptableObject
    {
        private const string ResourcesPath = "PlayerClassPresentationDatabase";

        [SerializeField] private PlayerClassPresentationDefinition[] presentations = Array.Empty<PlayerClassPresentationDefinition>();

        private static PlayerClassPresentationDatabase cached;
        private Dictionary<PlayerClassType, PlayerClassPresentationDefinition> lookup;

        public static PlayerClassPresentationDatabase Current
        {
            get
            {
                if (cached == null)
                {
                    cached = Resources.Load<PlayerClassPresentationDatabase>(ResourcesPath);
                }

                return cached;
            }
        }

        public static PlayerClassPresentationDefinition Find(PlayerClassType classType)
        {
            return Current != null ? Current.Get(classType) : null;
        }

        public PlayerClassPresentationDefinition Get(PlayerClassType classType)
        {
            EnsureLookup();
            return lookup.TryGetValue(classType, out var definition) ? definition : null;
        }

        private void EnsureLookup()
        {
            if (lookup != null)
            {
                return;
            }

            lookup = new Dictionary<PlayerClassType, PlayerClassPresentationDefinition>();
            foreach (var presentation in presentations)
            {
                if (presentation == null)
                {
                    continue;
                }

                lookup[presentation.ClassType] = presentation;
            }
        }

#if UNITY_EDITOR
        public void EditorSetPresentations(PlayerClassPresentationDefinition[] definitions)
        {
            presentations = definitions ?? Array.Empty<PlayerClassPresentationDefinition>();
            lookup = null;
            cached = this;
        }
#endif
    }
}
