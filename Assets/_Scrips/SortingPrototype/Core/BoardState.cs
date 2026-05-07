using System;
using System.Collections.Generic;

namespace SortingPrototype.Core
{
    [Serializable]
    public sealed class BoardState
    {
        private readonly List<BranchState> _branches;

        public BoardState(IEnumerable<BranchState> branches)
        {
            if (branches == null)
            {
                throw new ArgumentNullException(nameof(branches));
            }

            _branches = new List<BranchState>(branches);

            if (_branches.Count == 0)
            {
                throw new ArgumentException("Board must contain at least one branch.", nameof(branches));
            }
        }

        public int BranchCount => _branches.Count;
        public IReadOnlyList<BranchState> Branches => _branches;

        public BranchState GetBranch(int branchIndex)
        {
            if (branchIndex < 0 || branchIndex >= BranchCount)
            {
                throw new ArgumentOutOfRangeException(nameof(branchIndex));
            }

            return _branches[branchIndex];
        }

        public void ApplyMove(BoardMove move)
        {
            var source = GetBranch(move.SourceIndex);
            var target = GetBranch(move.TargetIndex);
            var removedPieces = source.PopTopPieces(move.PieceCount);
            target.PushPieces(removedPieces);
        }
    }
}
