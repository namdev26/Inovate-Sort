using System;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Config
{
    [Serializable]
    public sealed class SpritePaletteEntry
    {
        [SerializeField] private PieceColorId colorId = PieceColorId.None;
        [SerializeField] private Sprite[] sprites = Array.Empty<Sprite>();

        public PieceColorId ColorId => colorId;
        public Sprite[] Sprites => sprites;
    }
}

