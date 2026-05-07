using System;
using System.Collections.Generic;

namespace SortingPrototype.Core
{
    [Serializable]
    public sealed class BranchState
    {
        private readonly List<PieceColorId> _pieces;

        public BranchState(int capacity, IEnumerable<PieceColorId> initialPieces)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
            _pieces = initialPieces == null ? new List<PieceColorId>(capacity) : new List<PieceColorId>(initialPieces);

            if (_pieces.Count > capacity)
            {
                throw new ArgumentException("Initial piece count cannot exceed capacity.", nameof(initialPieces));
            }
        }

        public int Capacity { get; }
        public int PieceCount => _pieces.Count;
        public int AvailableSlots => Capacity - PieceCount;
        public bool IsEmpty => PieceCount == 0;
        public bool IsFull => PieceCount >= Capacity;
        public IReadOnlyList<PieceColorId> Pieces => _pieces;

        public bool TryGetTopColor(out PieceColorId colorId)
        {
            if (IsEmpty)
            {
                colorId = PieceColorId.None;
                return false;
            }

            colorId = _pieces[PieceCount - 1];
            return true;
        }

        public int GetTopGroupCount()
        {
            if (!TryGetTopColor(out var topColor))
            {
                return 0;
            }

            var count = 0;
            for (var index = PieceCount - 1; index >= 0; index--)
            {
                if (_pieces[index] != topColor)
                {
                    break;
                }

                count++;
            }

            return count;
        }

        public bool IsUniformAndFull()
        {
            if (!IsFull || !TryGetTopColor(out var topColor))
            {
                return false;
            }

            for (var index = 0; index < _pieces.Count; index++)
            {
                if (_pieces[index] != topColor)
                {
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyList<PieceColorId> PopTopPieces(int pieceCount)
        {
            ValidateRemoval(pieceCount);

            var startIndex = PieceCount - pieceCount;
            var removedPieces = _pieces.GetRange(startIndex, pieceCount);
            _pieces.RemoveRange(startIndex, pieceCount);
            return removedPieces;
        }

        public void PushPieces(IEnumerable<PieceColorId> pieces)
        {
            if (pieces == null)
            {
                throw new ArgumentNullException(nameof(pieces));
            }

            foreach (var piece in pieces)
            {
                if (IsFull)
                {
                    throw new InvalidOperationException("Branch capacity exceeded.");
                }

                _pieces.Add(piece);
            }
        }

        private void ValidateRemoval(int pieceCount)
        {
            if (pieceCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pieceCount));
            }

            if (pieceCount > PieceCount)
            {
                throw new InvalidOperationException("Cannot remove more pieces than the branch contains.");
            }
        }
    }
}
