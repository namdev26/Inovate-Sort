using System;
using System.Linq;
using SortingPrototype.Config;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Presentation
{
    public sealed class PrototypeBoardController : MonoBehaviour
    {
        [SerializeField] private PrototypeLevelDefinition levelDefinition;
        [SerializeField] private SpritePalette spritePalette;
        [SerializeField] private BranchView[] branchViews = Array.Empty<BranchView>();

        private BoardState _boardState;
        private IBoardRules _boardRules;
        private int? _selectedSourceIndex;

        private void Awake()
        {
            ResolveBranchViews();
            ValidateConfiguration();
            _boardRules = new StandardBoardRules();
            _boardState = BoardStateFactory.Create(levelDefinition);
            InitializeViews();
            RefreshBoard();
        }

        public void RestartBoard()
        {
            _selectedSourceIndex = null;
            _boardState = BoardStateFactory.Create(levelDefinition);
            RefreshBoard();
        }

        private void ResolveBranchViews()
        {
            if (branchViews != null && branchViews.Length > 0)
            {
                return;
            }

            branchViews = FindObjectsByType<BranchView>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .OrderBy(view => view.transform.position.x)
                .ThenByDescending(view => view.transform.position.y)
                .ToArray();
        }

        private void InitializeViews()
        {
            for (var index = 0; index < branchViews.Length; index++)
            {
                branchViews[index].Initialize(index, HandleBranchClicked);
                branchViews[index].SetSelected(false);
            }
        }

        private void HandleBranchClicked(int branchIndex)
        {
            if (!_selectedSourceIndex.HasValue)
            {
                TrySelectSource(branchIndex);
                return;
            }

            if (_selectedSourceIndex.Value == branchIndex)
            {
                ClearSelection();
                return;
            }

            TryMoveSelectedSource(branchIndex);
        }

        private void TrySelectSource(int branchIndex)
        {
            if (!_boardRules.CanSelectSource(_boardState, branchIndex))
            {
                return;
            }

            _selectedSourceIndex = branchIndex;
            UpdateSelectionVisuals();
        }

        private void TryMoveSelectedSource(int targetIndex)
        {
            if (!_selectedSourceIndex.HasValue)
            {
                return;
            }

            if (_boardRules.TryCreateMove(_boardState, _selectedSourceIndex.Value, targetIndex, out var move))
            {
                _boardState.ApplyMove(move);
            }

            ClearSelection();
            RefreshBoard();

            if (_boardRules.IsSolved(_boardState))
            {
                Debug.Log("Prototype solved.", this);
            }
        }

        private void RefreshBoard()
        {
            for (var index = 0; index < branchViews.Length; index++)
            {
                var branch = _boardState.GetBranch(index);
                branchViews[index].Render(branch.Pieces, token => spritePalette.GetSprite(token.ColorId, token.Variant));
            }

            UpdateSelectionVisuals();
        }

        private void ClearSelection()
        {
            _selectedSourceIndex = null;
            UpdateSelectionVisuals();
        }

        private void UpdateSelectionVisuals()
        {
            for (var index = 0; index < branchViews.Length; index++)
            {
                branchViews[index].SetSelected(_selectedSourceIndex.HasValue && _selectedSourceIndex.Value == index);
            }
        }

        private void ValidateConfiguration()
        {
            if (levelDefinition == null)
            {
                throw new InvalidOperationException("PrototypeBoardController requires a level definition.");
            }

            if (spritePalette == null)
            {
                throw new InvalidOperationException("PrototypeBoardController requires a sprite palette.");
            }

            if (branchViews == null || branchViews.Length == 0)
            {
                throw new InvalidOperationException("PrototypeBoardController could not find any BranchView objects in the scene.");
            }

            if (branchViews.Length != levelDefinition.BranchCount)
            {
                throw new InvalidOperationException($"Branch view count ({branchViews.Length}) must match the level branch count ({levelDefinition.BranchCount}).");
            }
        }
    }
}
