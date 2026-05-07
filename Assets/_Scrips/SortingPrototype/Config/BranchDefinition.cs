using System;
using System.Collections.Generic;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Config
{
    [Serializable]
    public sealed class BranchDefinition
    {
        [SerializeField] private List<PieceColorId> pieces = new();

        public IReadOnlyList<PieceColorId> Pieces => pieces;
    }
}
