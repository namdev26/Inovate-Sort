using System.Collections.Generic;
using SortingPrototype.Config;

namespace SortingPrototype.Core
{
    public static class BoardStateFactory
    {
        public static BoardState Create(PrototypeLevelDefinition levelDefinition)
        {
            var branches = new List<BranchState>(levelDefinition.BranchCount);
            var legacyVariantCounters = new Dictionary<PieceColorId, int>();
            foreach (var branchDefinition in levelDefinition.Branches)
            {
                if (branchDefinition.PieceTokens != null && branchDefinition.PieceTokens.Count > 0)
                {
                    branches.Add(new BranchState(levelDefinition.BranchCapacity, branchDefinition.GetInitialTokens()));
                    continue;
                }

                // Legacy fallback: assign variants per ColorId across entire level
                var legacyTokens = new List<PieceToken>(branchDefinition.LegacyColorIds.Count);
                foreach (var colorId in branchDefinition.LegacyColorIds)
                {
                    if (!legacyVariantCounters.TryGetValue(colorId, out var counter))
                    {
                        counter = 0;
                    }

                    legacyTokens.Add(new PieceToken(colorId, counter % 4));
                    legacyVariantCounters[colorId] = counter + 1;
                }

                branches.Add(new BranchState(levelDefinition.BranchCapacity, legacyTokens));
            }

            return new BoardState(branches);
        }
    }
}
