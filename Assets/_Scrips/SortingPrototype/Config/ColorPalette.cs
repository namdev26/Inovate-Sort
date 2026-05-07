using System;
using System.Collections.Generic;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Config
{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Sorting Prototype/Color Palette")]
    public sealed class ColorPalette : ScriptableObject
    {
        [SerializeField] private Color fallbackColor = Color.white;
        [SerializeField] private List<ColorPaletteEntry> entries = new();

        private Dictionary<PieceColorId, Color> _cachedLookup;

        public Color GetColor(PieceColorId colorId)
        {
            if (colorId == PieceColorId.None)
            {
                return fallbackColor;
            }

            EnsureLookup();
            return _cachedLookup.TryGetValue(colorId, out var resolvedColor) ? resolvedColor : fallbackColor;
        }

        private void EnsureLookup()
        {
            if (_cachedLookup != null)
            {
                return;
            }

            _cachedLookup = new Dictionary<PieceColorId, Color>();
            foreach (var entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                _cachedLookup[entry.ColorId] = entry.Color;
            }
        }

        private void OnValidate()
        {
            _cachedLookup = null;
        }
    }
}
