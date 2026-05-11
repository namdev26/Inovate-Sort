using System;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Config
{
    [Serializable]
    public sealed class PieceTokenDefinition
    {
        [SerializeField] private PieceColorId colorId = PieceColorId.None;
        [SerializeField, Min(0)] private int variant;

        public PieceColorId ColorId => colorId;
        public int Variant => variant;

        public PieceToken ToToken()
        {
            return new PieceToken(colorId, variant);
        }
    }
}

