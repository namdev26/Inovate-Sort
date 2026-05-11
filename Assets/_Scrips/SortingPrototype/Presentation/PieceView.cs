using UnityEngine;

namespace SortingPrototype.Presentation
{
    public sealed class PieceView : MonoBehaviour
    {
        private const float DefaultPunchStrength = 0.12f;
        private const float DefaultPunchDurationSeconds = 0.12f;
        private const float DefaultOutlineFadeSeconds = 0.08f;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer outlineRenderer;
        [SerializeField, Range(0f, 1f)] private float punchStrength = DefaultPunchStrength;
        [SerializeField, Min(0.01f)] private float punchDurationSeconds = DefaultPunchDurationSeconds;
        [SerializeField, Min(0.01f)] private float outlineFadeSeconds = DefaultOutlineFadeSeconds;

        private Vector3 _baseScale;
        private bool _isSelected;
        private Coroutine _punchRoutine;
        private Coroutine _outlineFadeRoutine;

        public void SetSprite(Sprite sprite)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
        }

        public void SetSelected(bool isSelected)
        {
            if (_isSelected == isSelected)
            {
                return;
            }

            _isSelected = isSelected;

            if (outlineRenderer != null)
            {
                StartOutlineFade(isSelected);
            }

            if (isSelected)
            {
                PlayPunch();
            }
        }

        public void PlayPunch()
        {
            EnsureBaseScale();
            if (_punchRoutine != null)
            {
                StopCoroutine(_punchRoutine);
            }

            _punchRoutine = StartCoroutine(PunchRoutine());
        }

        private void Awake()
        {
            EnsureBaseScale();
            if (outlineRenderer != null)
            {
                // Ensure outline starts hidden but with a valid color state for fading.
                var c = outlineRenderer.color;
                outlineRenderer.color = new Color(c.r, c.g, c.b, 0f);
                outlineRenderer.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (_punchRoutine != null)
            {
                StopCoroutine(_punchRoutine);
                _punchRoutine = null;
            }

            if (_outlineFadeRoutine != null)
            {
                StopCoroutine(_outlineFadeRoutine);
                _outlineFadeRoutine = null;
            }

            _isSelected = false;
            EnsureBaseScale();
            transform.localScale = _baseScale;

            if (outlineRenderer != null)
            {
                var c = outlineRenderer.color;
                outlineRenderer.color = new Color(c.r, c.g, c.b, 0f);
                outlineRenderer.gameObject.SetActive(false);
            }
        }

        private void EnsureBaseScale()
        {
            if (_baseScale == default)
            {
                _baseScale = transform.localScale;
            }
        }

        private System.Collections.IEnumerator PunchRoutine()
        {
            var duration = Mathf.Max(0.01f, punchDurationSeconds);
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var normalized = Mathf.Clamp01(t / duration);
                var pulse = Mathf.Sin(normalized * Mathf.PI);
                var factor = 1f + pulse * Mathf.Clamp01(punchStrength);
                transform.localScale = _baseScale * factor;
                yield return null;
            }

            transform.localScale = _baseScale;
            _punchRoutine = null;
        }

        private void StartOutlineFade(bool fadeIn)
        {
            if (_outlineFadeRoutine != null)
            {
                StopCoroutine(_outlineFadeRoutine);
            }

            _outlineFadeRoutine = StartCoroutine(OutlineFadeRoutine(fadeIn));
        }

        private System.Collections.IEnumerator OutlineFadeRoutine(bool fadeIn)
        {
            if (outlineRenderer == null)
            {
                yield break;
            }

            outlineRenderer.gameObject.SetActive(true);

            var duration = Mathf.Max(0.01f, outlineFadeSeconds);
            var startColor = outlineRenderer.color;
            var startA = startColor.a;
            var endA = fadeIn ? 1f : 0f;

            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var normalized = Mathf.Clamp01(t / duration);
                var a = Mathf.Lerp(startA, endA, normalized);
                outlineRenderer.color = new Color(startColor.r, startColor.g, startColor.b, a);
                yield return null;
            }

            outlineRenderer.color = new Color(startColor.r, startColor.g, startColor.b, endA);
            if (!fadeIn)
            {
                outlineRenderer.gameObject.SetActive(false);
            }

            _outlineFadeRoutine = null;
        }
    }
}
