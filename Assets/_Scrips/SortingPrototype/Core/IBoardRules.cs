namespace SortingPrototype.Core
{
    public interface IBoardRules
    {
        bool CanSelectSource(BoardState boardState, int branchIndex);
        bool TryCreateMove(BoardState boardState, int sourceIndex, int targetIndex, out BoardMove move);
        bool IsSolved(BoardState boardState);
    }
}
