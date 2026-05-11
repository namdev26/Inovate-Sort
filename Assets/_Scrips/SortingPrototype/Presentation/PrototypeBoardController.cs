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
        [SerializeField, Min(0.01f)] private float assembleDurationSeconds = 0.35f;
        [SerializeField, Min(0.01f)] private float moveDurationSeconds = 0.22f;
        [SerializeField] private Transform moveAnimationRoot;

        private BoardState _boardState;
        private IBoardRules _boardRules;
        private int? _selectedSourceIndex;
        private bool[] _assembledBranches;
        private bool _inputLocked;
        private int _activeAssembleCount;
        private Coroutine _moveRoutine;

        private void Awake()
        {
            ResolveBranchViews();
            ValidateConfiguration();
            _boardRules = new StandardBoardRules();
            _boardState = BoardStateFactory.Create(levelDefinition);
            _assembledBranches = new bool[_boardState.BranchCount];
            InitializeViews();
            RefreshBoard();
        }

        public void RestartBoard()
        {
            _selectedSourceIndex = null;
            _boardState = BoardStateFactory.Create(levelDefinition);
            _assembledBranches = new bool[_boardState.BranchCount];
            _inputLocked = false;
            _activeAssembleCount = 0;
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
            if (_inputLocked)
            {
                return;
            }

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
                StartMoveAnimation(move);
            }

            ClearSelection();
        }

        private void StartMoveAnimation(BoardMove move)
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }

            EnsureMoveAnimationRoot();
            _inputLocked = true;
            _moveRoutine = StartCoroutine(PlayMoveRoutine(move));
        }

        private System.Collections.IEnumerator PlayMoveRoutine(BoardMove move)
        {
            var sourceBranch = _boardState.GetBranch(move.SourceIndex);
            var targetBranch = _boardState.GetBranch(move.TargetIndex);

            var sourceCount = sourceBranch.PieceCount;
            var targetStartIndex = targetBranch.PieceCount;

            branchViews[move.SourceIndex].HideTopPiecesTemporarily(move.PieceCount);
            RefreshBoard();

            var movingTokens = sourceBranch.Pieces
                .Skip(Mathf.Max(0, sourceCount - move.PieceCount))
                .Take(move.PieceCount)
                .ToArray();

            var isMovingWholePicture = IsWholePictureMove(move, movingTokens);

            var flyingPrefab = branchViews[move.SourceIndex].PiecePrefab;
            var flyers = new PieceView[movingTokens.Length];
            var from = new Vector3[movingTokens.Length];
            var to = new Vector3[movingTokens.Length];

            for (var i = 0; i < movingTokens.Length; i++)
            {
                var token = movingTokens[i];
                flyers[i] = Instantiate(flyingPrefab, moveAnimationRoot);
                flyers[i].SetSelected(false);
                flyers[i].SetSprite(spritePalette.GetSprite(token.ColorId, token.Variant));

                var sourceSlot = sourceCount - movingTokens.Length + i;
                var targetSlot = targetStartIndex + i;
                from[i] = branchViews[move.SourceIndex].IsAssembled
                    ? branchViews[move.SourceIndex].GetWorldPositionForVariant(token.Variant)
                    : branchViews[move.SourceIndex].GetWorldPositionForSlot(sourceSlot);
                to[i] = isMovingWholePicture
                    ? branchViews[move.TargetIndex].GetWorldPositionForVariant(token.Variant)
                    : branchViews[move.TargetIndex].GetWorldPositionForSlot(targetSlot);
                flyers[i].transform.position = from[i];
            }

            var duration = Mathf.Max(0.01f, moveDurationSeconds);
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var normalized = Mathf.Clamp01(t / duration);
                var eased = normalized * normalized * (3f - 2f * normalized);
                for (var i = 0; i < flyers.Length; i++)
                {
                    if (flyers[i] == null) continue;
                    flyers[i].transform.position = Vector3.LerpUnclamped(from[i], to[i], eased);
                }

                yield return null;
            }

            for (var i = 0; i < flyers.Length; i++)
            {
                if (flyers[i] != null)
                {
                    Destroy(flyers[i].gameObject);
                }
            }

            _boardState.ApplyMove(move);
            branchViews[move.SourceIndex].ClearTemporaryHiddenPieces();

            if (isMovingWholePicture)
            {
                branchViews[move.TargetIndex].SetAssembled(true);
            }

            RefreshBoard();

            if (_boardRules.IsSolved(_boardState))
            {
                Debug.Log("Prototype solved.", this);
            }

            _moveRoutine = null;
            _inputLocked = false;
        }

        private static bool IsWholePictureMove(BoardMove move, PieceToken[] movingTokens)
        {
            if (move.PieceCount != BranchPictureCompletion.PictureWidth || movingTokens == null || movingTokens.Length != BranchPictureCompletion.PictureWidth)
            {
                return false;
            }

            var mask = 0;
            var color = movingTokens[0].ColorId;
            for (var i = 0; i < movingTokens.Length; i++)
            {
                if (movingTokens[i].ColorId != color)
                {
                    return false;
                }

                var v = movingTokens[i].Variant;
                if (v < 0 || v >= BranchPictureCompletion.PictureWidth)
                {
                    return false;
                }

                var bit = 1 << v;
                if ((mask & bit) != 0)
                {
                    return false;
                }

                mask |= bit;
            }

            return mask == 0b1111;
        }

        private void EnsureMoveAnimationRoot()
        {
            if (moveAnimationRoot != null)
            {
                return;
            }

            var go = new GameObject("MoveAnimationRoot");
            go.transform.SetParent(transform, false);
            moveAnimationRoot = go.transform;
        }

        private void RefreshBoard()
        {
            for (var index = 0; index < branchViews.Length; index++)
            {
                var branch = _boardState.GetBranch(index);
                branchViews[index].Render(branch.Pieces, token => spritePalette.GetSprite(token.ColorId, token.Variant));
                TryAssembleBranchIfJustCompleted(index, branch);
            }

            UpdateSelectionVisuals();
        }

        private void TryAssembleBranchIfJustCompleted(int branchIndex, BranchState branch)
        {
            if (_assembledBranches == null || branchIndex < 0 || branchIndex >= _assembledBranches.Length)
            {
                return;
            }

            if (_assembledBranches[branchIndex])
            {
                return;
            }

            if (!BranchPictureCompletion.IsPicture4x1Complete(branch))
            {
                return;
            }

            _assembledBranches[branchIndex] = true;
            _activeAssembleCount++;
            _inputLocked = true;

            var snapshot = branch.Pieces.ToArray();
            branchViews[branchIndex].PlayAssemble4x1(
                snapshot,
                token => spritePalette.GetSprite(token.ColorId, token.Variant),
                assembleDurationSeconds,
                HandleAssembleCompleted
            );
        }

        private void HandleAssembleCompleted()
        {
            _activeAssembleCount = Mathf.Max(0, _activeAssembleCount - 1);
            _inputLocked = _activeAssembleCount > 0;
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
