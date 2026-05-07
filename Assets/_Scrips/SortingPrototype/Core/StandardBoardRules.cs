using System;

namespace SortingPrototype.Core
{
    public sealed class StandardBoardRules : IBoardRules
    {
        public bool CanSelectSource(BoardState boardState, int branchIndex)
        {
            var branch = boardState.GetBranch(branchIndex);
            return !branch.IsEmpty;
        }

        public bool TryCreateMove(BoardState boardState, int sourceIndex, int targetIndex, out BoardMove move)
        {
            move = default;

            if (sourceIndex == targetIndex)
            {
                return false;
            }

            var source = boardState.GetBranch(sourceIndex);
            var target = boardState.GetBranch(targetIndex);

            if (!source.TryGetTopColor(out var movingColor) || target.IsFull)
            {
                return false;
            }

            if (!CanPlaceOnTarget(target, movingColor))
            {
                return false;
            }

            var moveCount = Math.Min(source.GetTopGroupCount(), target.AvailableSlots);
            if (moveCount <= 0)
            {
                return false;
            }

            move = new BoardMove(sourceIndex, targetIndex, movingColor, moveCount);
            return true;
        }

        public bool IsSolved(BoardState boardState)
        {
            for (var index = 0; index < boardState.BranchCount; index++)
            {
                var branch = boardState.GetBranch(index);
                if (branch.IsEmpty)
                {
                    continue;
                }

                if (!branch.IsUniformAndFull())
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CanPlaceOnTarget(BranchState target, PieceColorId movingColor)
        {
            if (target.IsEmpty)
            {
                return true;
            }

            return target.TryGetTopColor(out var targetTopColor) && targetTopColor == movingColor;
        }
    }
}
