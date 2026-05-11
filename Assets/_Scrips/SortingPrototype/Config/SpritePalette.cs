using System.Collections.Generic;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Config
{
    [CreateAssetMenu(fileName = "SpritePalette", menuName = "Sorting Prototype/Sprite Palette")]
    public sealed class SpritePalette : ScriptableObject
    {
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private List<SpritePaletteEntry> entries = new();

        private Dictionary<PieceColorId, Sprite[]> _cachedLookup;

        public Sprite GetSprite(PieceColorId colorId, int variant)
        {
            if (colorId == PieceColorId.None)
            {
                return fallbackSprite;
            }

            EnsureLookup();
            if (!_cachedLookup.TryGetValue(colorId, out var sprites) || sprites == null || sprites.Length == 0)
            {
                return fallbackSprite;
            }

            if (variant < 0 || variant >= sprites.Length)
            {
                return fallbackSprite;
            }

            return sprites[variant] != null ? sprites[variant] : fallbackSprite;
        }

        private void EnsureLookup()
        {
            if (_cachedLookup != null)
            {
                return;
            }

            _cachedLookup = new Dictionary<PieceColorId, Sprite[]>();
            foreach (var entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                _cachedLookup[entry.ColorId] = entry.Sprites;
            }
        }

        private void OnValidate()
        {
            _cachedLookup = null;
        }
    }
}

