using UnityEngine;

namespace SortingPrototype.Presentation
{
    public sealed class PieceView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetColor(Color color)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = color;
        }
    }
}
