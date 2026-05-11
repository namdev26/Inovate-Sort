using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace SortingPrototype.Config
{
    [Serializable]
    public sealed class BranchDefinition
    {
        // Backward-compatible: old levels serialized PieceColorId list under field name "pieces".
        // This lets Unity load old YAML into this field, while new data uses `pieceTokens`.
        [FormerlySerializedAs("pieces")]
        [SerializeField]
        private List<SortingPrototype.Core.PieceColorId> legacyColorIds = new();

        [SerializeField] private List<PieceTokenDefinition> pieceTokens = new();

        public IReadOnlyList<PieceTokenDefinition> PieceTokens => pieceTokens;

        public IReadOnlyList<SortingPrototype.Core.PieceColorId> LegacyColorIds => legacyColorIds;

        public IEnumerable<SortingPrototype.Core.PieceToken> GetInitialTokens()
        {
            if (pieceTokens != null && pieceTokens.Count > 0)
            {
                return pieceTokens.Where(p => p != null).Select(p => p.ToToken());
            }

            if (legacyColorIds != null && legacyColorIds.Count > 0)
            {
                return legacyColorIds.Select(colorId => new SortingPrototype.Core.PieceToken(colorId, 0));
            }

            return Array.Empty<SortingPrototype.Core.PieceToken>();
        }
    }
}
