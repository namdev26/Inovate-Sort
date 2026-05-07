using System;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Config
{
    [Serializable]
    public sealed class ColorPaletteEntry
    {
        [SerializeField] private PieceColorId colorId = PieceColorId.None;
        [SerializeField] private Color color = Color.white;

        public PieceColorId ColorId => colorId;
        public Color Color => color;
    }
}
