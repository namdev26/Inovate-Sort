using System.Collections.Generic;
using SortingPrototype.Config;

namespace SortingPrototype.Core
{
    public static class BoardStateFactory
    {
        public static BoardState Create(PrototypeLevelDefinition levelDefinition)
        {
            var branches = new List<BranchState>(levelDefinition.BranchCount);
            foreach (var branchDefinition in levelDefinition.Branches)
            {
                branches.Add(new BranchState(levelDefinition.BranchCapacity, branchDefinition.Pieces));
            }

            return new BoardState(branches);
        }
    }
}
