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

        public void Initialize(int branchIndex, Action<int> clickHandler)
        {
            EnsureReferences();
            maxSlotCount = Mathf.Max(1, maxSlotCount);
            _branchIndex = branchIndex;
            _clickHandler = clickHandler;
        }

        public void Render(IReadOnlyList<PieceColorId> pieces, Func<PieceColorId, Color> colorResolver)
        {
            EnsureReferences();
            EnsurePieceCount(pieces.Count);

            for (var index = 0; index < _spawnedPieces.Count; index++)
            {
                var isActive = index < pieces.Count;
                _spawnedPieces[index].gameObject.SetActive(isActive);
                if (!isActive)
                {
                    continue;
                }

                _spawnedPieces[index].transform.localPosition = GetPieceLocalPosition(index);
                _spawnedPieces[index].SetColor(colorResolver.Invoke(pieces[index]));
            }
        }

        public void SetSelected(bool isSelected)
        {
            EnsureReferences();
            if (highlightRenderer == null)
            {
                return;
            }

            highlightRenderer.gameObject.SetActive(isSelected);
        }

        private void OnMouseDown()
        {
            _clickHandler?.Invoke(_branchIndex);
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
