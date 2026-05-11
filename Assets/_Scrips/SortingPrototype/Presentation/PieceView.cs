using UnityEngine;

namespace SortingPrototype.Presentation
{
    public sealed class PieceView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer outlineRenderer;

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
            if (outlineRenderer == null)
            {
                return;
            }

            outlineRenderer.gameObject.SetActive(isSelected);
        }
    }
}
