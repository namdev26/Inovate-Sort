using System;

namespace SortingPrototype.Core
{
    [Serializable]
    public readonly struct PieceToken
    {
        public PieceToken(PieceColorId colorId, int variant)
        {
            ColorId = colorId;
            Variant = variant;
        }

        public PieceColorId ColorId { get; }
        public int Variant { get; }
    }
}

