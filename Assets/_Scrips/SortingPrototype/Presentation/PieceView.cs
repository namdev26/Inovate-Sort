using UnityEngine;

namespace SortingPrototype.Presentation
{
    public sealed class PieceView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer outlineRenderer;

        public void SetColor(Color color)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = color;
        }

        public void SetSelected(bool isSelected)
        {
            if (outlineRenderer == null)
            {
                return;
            }

            outlineRenderer.gameObject.SetActive(isSelected);
        }
    }
}
