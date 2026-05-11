using System;
using System.Collections.Generic;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BranchView : MonoBehaviour
    {
        private const string HighlightChildName = "Highlight";
        private const string PieceRootChildName = "PieceRoot";

        [SerializeField] private SpriteRenderer highlightRenderer;
        [SerializeField] private Transform pieceRoot;
        [SerializeField] private PieceView piecePrefab;
        [SerializeField, Min(1)] private int maxSlotCount = 4;
        [SerializeField] private float horizontalSpacing = 0.8f;
        [SerializeField] private bool mirrorLayoutOnRightSide = true;

        private readonly List<PieceView> _spawnedPieces = new();
        private Action<int> _clickHandler;
        private int _branchIndex;
        private bool _isSelected;
        private int _highlightStartIndex = -1;
        private int _highlightEndIndex = -1;

        public void Initialize(int branchIndex, Action<int> clickHandler)
        {
            EnsureReferences();
            maxSlotCount = Mathf.Max(1, maxSlotCount);
            _branchIndex = branchIndex;
            _clickHandler = clickHandler;
        }

        public void Render(IReadOnlyList<PieceToken> pieces, Func<PieceToken, Sprite> spriteResolver)
        {
            EnsureReferences();
            EnsurePieceCount(pieces.Count);
            UpdateHighlightRange(pieces);

            for (var index = 0; index < _spawnedPieces.Count; index++)
            {
                var isActive = index < pieces.Count;
                var pieceView = _spawnedPieces[index];
                pieceView.gameObject.SetActive(isActive);
                pieceView.SetSelected(_isSelected && IsHighlightedPiece(index));
                if (!isActive)
                {
                    continue;
                }

                pieceView.transform.localPosition = GetPieceLocalPosition(index);
                pieceView.SetSprite(spriteResolver.Invoke(pieces[index]));
            }
        }

        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            if (highlightRenderer != null)
            {
                highlightRenderer.gameObject.SetActive(false);
            }

            UpdatePieceSelectionVisuals();
        }

        private void OnMouseDown()
        {
            PlayClickFeedback();
            _clickHandler?.Invoke(_branchIndex);
        }

        private void PlayClickFeedback()
        {
            if (_highlightStartIndex < 0 || _highlightEndIndex < 0)
            {
                return;
            }

            for (var i = _highlightStartIndex; i <= _highlightEndIndex; i++)
            {
                if (i < 0 || i >= _spawnedPieces.Count)
                {
                    continue;
                }

                var piece = _spawnedPieces[i];
                if (piece == null || !piece.gameObject.activeInHierarchy)
                {
                    continue;
                }

                piece.PlayPunch();
            }
        }

        private void EnsureReferences()
        {
            if (pieceRoot == null)
            {
                var pieceRootTransform = transform.Find(PieceRootChildName);
                if (pieceRootTransform != null)
                {
                    pieceRoot = pieceRootTransform;
                }
            }

            if (highlightRenderer == null)
            {
                var highlightTransform = transform.Find(HighlightChildName);
                if (highlightTransform != null)
                {
                    highlightRenderer = highlightTransform.GetComponent<SpriteRenderer>();
                }
            }
        }

        private Vector3 GetPieceLocalPosition(int pieceIndex)
        {
            var safeSlotCount = Mathf.Max(1, maxSlotCount);
            var rowWidth = (safeSlotCount - 1) * horizontalSpacing;
            var startX = -rowWidth * 0.5f;
            var positionX = startX + pieceIndex * horizontalSpacing;

            if (ShouldMirrorLayout())
            {
                positionX *= -1f;
            }

            return new Vector3(positionX, 0f, 0f);
        }

        private bool ShouldMirrorLayout()
        {
            return mirrorLayoutOnRightSide && transform.position.x > 0f;
        }

        private void UpdatePieceSelectionVisuals()
        {
            for (var index = 0; index < _spawnedPieces.Count; index++)
            {
                _spawnedPieces[index].SetSelected(_isSelected && IsHighlightedPiece(index));
            }
        }

        private void UpdateHighlightRange(IReadOnlyList<PieceToken> pieces)
        {
            _highlightStartIndex = -1;
            _highlightEndIndex = -1;
            if (pieces.Count == 0)
            {
                return;
            }

            var topColor = pieces[pieces.Count - 1].ColorId;
            _highlightEndIndex = pieces.Count - 1;
            _highlightStartIndex = _highlightEndIndex;
            for (var index = _highlightEndIndex - 1; index >= 0; index--)
            {
                if (pieces[index].ColorId != topColor)
                {
                    break;
                }

                _highlightStartIndex = index;
            }
        }

        private bool IsHighlightedPiece(int pieceIndex)
        {
            return pieceIndex >= _highlightStartIndex && pieceIndex <= _highlightEndIndex;
        }

        private void EnsurePieceCount(int targetCount)
        {
            while (_spawnedPieces.Count < targetCount)
            {
                if (piecePrefab == null || pieceRoot == null)
                {
                    throw new InvalidOperationException("BranchView requires a piece prefab and piece root.");
                }

                var pieceView = Instantiate(piecePrefab, pieceRoot);
                _spawnedPieces.Add(pieceView);
            }
        }
    }
}
