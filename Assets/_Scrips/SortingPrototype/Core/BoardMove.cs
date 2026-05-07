using System;

namespace SortingPrototype.Core
{
    [Serializable]
    public readonly struct BoardMove
    {
        public BoardMove(int sourceIndex, int targetIndex, PieceColorId colorId, int pieceCount)
        {
            SourceIndex = sourceIndex;
            TargetIndex = targetIndex;
            ColorId = colorId;
            PieceCount = pieceCount;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }
        public PieceColorId ColorId { get; }
        public int PieceCount { get; }
    }
}
