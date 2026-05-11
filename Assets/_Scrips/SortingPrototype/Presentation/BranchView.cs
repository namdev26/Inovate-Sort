using System;
using System.Collections.Generic;
using System.Linq;
using SortingPrototype.Core;
using UnityEngine;

namespace SortingPrototype.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BranchView : MonoBehaviour
    {
        private const string HighlightChildName = "Highlight";
        private const string PieceRootChildName = "PieceRoot";
        private const int PictureWidth = 4;

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
        private bool _layoutLocked;
        private bool _isAssembled;
        private Coroutine _assembleRoutine;
        private int _temporarilyHiddenTopCount;

        public PieceView PiecePrefab => piecePrefab;

        public void Initialize(int branchIndex, Action<int> clickHandler)
        {
            EnsureReferences();
            maxSlotCount = Mathf.Max(1, maxSlotCount);
            _branchIndex = branchIndex;
            _clickHandler = clickHandler;
            _isAssembled = false;
            _layoutLocked = false;
        }

        public void Render(IReadOnlyList<PieceToken> pieces, Func<PieceToken, Sprite> spriteResolver)
        {
            EnsureReferences();
            EnsurePieceCount(pieces.Count);
            UpdateHighlightRange(pieces);

            for (var index = 0; index < _spawnedPieces.Count; index++)
            {
                var isActive = index < pieces.Count;
                if (_temporarilyHiddenTopCount > 0 && index >= pieces.Count - _temporarilyHiddenTopCount)
                {
                    isActive = false;
                }
                var pieceView = _spawnedPieces[index];
                pieceView.gameObject.SetActive(isActive);
                pieceView.SetSelected(_isSelected && IsHighlightedPiece(index));
                if (!isActive)
                {
                    continue;
                }

                pieceView.SetSprite(spriteResolver.Invoke(pieces[index]));
            }

            if (_layoutLocked)
            {
                return;
            }

            if (_isAssembled && pieces.Count == PictureWidth)
            {
                ApplyAssembledLayout(pieces);
                return;
            }

            for (var index = 0; index < pieces.Count; index++)
            {
                if (_temporarilyHiddenTopCount > 0 && index >= pieces.Count - _temporarilyHiddenTopCount)
                {
                    continue;
                }
                _spawnedPieces[index].transform.localPosition = GetPieceLocalPosition(index);
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

        private void OnDisable()
        {
            if (_assembleRoutine != null)
            {
                StopCoroutine(_assembleRoutine);
                _assembleRoutine = null;
            }

            _layoutLocked = false;
            _temporarilyHiddenTopCount = 0;
        }

        public void HideTopPiecesTemporarily(int count)
        {
            _temporarilyHiddenTopCount = Mathf.Max(0, count);
        }

        public void ClearTemporaryHiddenPieces()
        {
            _temporarilyHiddenTopCount = 0;
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

        public Vector3 GetWorldPositionForSlot(int slotIndex)
        {
            EnsureReferences();
            var local = GetPieceLocalPosition(slotIndex);
            return pieceRoot != null ? pieceRoot.TransformPoint(local) : transform.TransformPoint(local);
        }

        public void PlayAssemble4x1(
            IReadOnlyList<PieceToken> pieces,
            Func<PieceToken, Sprite> spriteResolver,
            float durationSeconds,
            Action onComplete)
        {
            if (pieces == null || pieces.Count != PictureWidth)
            {
                onComplete?.Invoke();
                return;
            }

            EnsureReferences();
            EnsurePieceCount(pieces.Count);

            _isAssembled = true;

            if (_assembleRoutine != null)
            {
                StopCoroutine(_assembleRoutine);
            }

            _assembleRoutine = StartCoroutine(Assemble4x1Routine(pieces, spriteResolver, durationSeconds, onComplete));
        }

        private System.Collections.IEnumerator Assemble4x1Routine(
            IReadOnlyList<PieceToken> pieces,
            Func<PieceToken, Sprite> spriteResolver,
            float durationSeconds,
            Action onComplete)
        {
            _layoutLocked = true;

            for (var i = 0; i < PictureWidth; i++)
            {
                _spawnedPieces[i].gameObject.SetActive(true);
                _spawnedPieces[i].SetSelected(false);
                _spawnedPieces[i].SetSprite(spriteResolver.Invoke(pieces[i]));
            }

            var order = Enumerable
                .Range(0, PictureWidth)
                .Select(index => new { index, variant = pieces[index].Variant })
                .OrderBy(x => ShouldMirrorLayout() ? -x.variant : x.variant)
                .ToArray();

            var targets = GetAssembledLocalTargets(order.Length);
            var movers = order.Select(x => _spawnedPieces[x.index]).ToArray();
            var startPositions = movers.Select(p => p.transform.localPosition).ToArray();

            var duration = Mathf.Max(0.01f, durationSeconds);
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var normalized = Mathf.Clamp01(t / duration);
                var eased = SmoothStep(normalized);
                for (var i = 0; i < movers.Length; i++)
                {
                    movers[i].transform.localPosition = Vector3.LerpUnclamped(startPositions[i], targets[i], eased);
                }

                yield return null;
            }

            for (var i = 0; i < movers.Length; i++)
            {
                movers[i].transform.localPosition = targets[i];
            }

            _layoutLocked = false;
            _assembleRoutine = null;
            onComplete?.Invoke();
        }

        private void ApplyAssembledLayout(IReadOnlyList<PieceToken> pieces)
        {
            var order = Enumerable
                .Range(0, PictureWidth)
                .Select(index => new { index, variant = pieces[index].Variant })
                .OrderBy(x => ShouldMirrorLayout() ? -x.variant : x.variant)
                .ToArray();

            var targets = GetAssembledLocalTargets(order.Length);
            for (var i = 0; i < order.Length; i++)
            {
                _spawnedPieces[order[i].index].transform.localPosition = targets[i];
            }
        }

        private Vector3[] GetAssembledLocalTargets(int count)
        {
            var targets = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                targets[i] = GetPieceLocalPosition(i);
            }

            return targets;
        }

        private static float SmoothStep(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
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
