using UnityEngine;
using UnityEngine.EventSystems;

namespace Antibody
{
    public class ScrollItemBehaviour : UIBehaviour
    {
        public void UpdateItem<T>(T item) where T : ScrollItem
        {
            var rect = transform as RectTransform;
            rect.sizeDelta = item.SizeDelta;
        }
    }
    public class ScrollItem
    {
        public Vector2 SizeDelta { get; private set; }
        public ScrollItem(Vector2 _sizeDelta)
        {
            SizeDelta = _sizeDelta;
        }
    }
    public interface IScrollItemSetUp
    {
        void UpdateItem<T>(int count,T item, GameObject obj) where T : ScrollItem;
    }
}