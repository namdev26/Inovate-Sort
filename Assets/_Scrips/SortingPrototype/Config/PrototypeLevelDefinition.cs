using System;
using System.Collections.Generic;
using UnityEngine;

namespace SortingPrototype.Config
{
    [CreateAssetMenu(fileName = "PrototypeLevelDefinition", menuName = "Sorting Prototype/Level Definition")]
    public sealed class PrototypeLevelDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int branchCapacity = 4;
        [SerializeField] private List<BranchDefinition> branches = new();

        public int BranchCapacity => branchCapacity;
        public int BranchCount => branches.Count;
        public IReadOnlyList<BranchDefinition> Branches => branches;

        private void OnValidate()
        {
            branchCapacity = Mathf.Max(1, branchCapacity);
        }
    }
}
