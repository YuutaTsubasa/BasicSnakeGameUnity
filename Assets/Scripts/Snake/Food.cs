using UnityEngine;

namespace Yuuta.Snake
{
    public class Food : MonoBehaviour
    {
        [SerializeField] private RectTransform _groundRectTransform;

        public void RandomPut()
        {
            GetComponent<RectTransform>()
                .anchoredPosition = new Vector2(
                Random.Range(0, _groundRectTransform.rect.width),
                Random.Range(0, _groundRectTransform.rect.height));
        }
        
    }
}

