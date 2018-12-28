using System.Linq;
using UnityEngine;
using Scrmizu;
using UnityEngine.UI;

namespace Scrmizu_Sample
{
    [RequireComponent(typeof(RectTransform))]
    public class VerticalScrollItem : MonoBehaviour, IInfiniteScrollItem
    {
        private VerticalScrollItemData _data;

        private Text _title;
        private Text _count;
        private Text _value;

        private Text Title => _title != null
            ? _title
            : _title = GetComponentsInChildren<Text>().FirstOrDefault(text => text.name == "Title");

        private Text Count => _count != null
            ? _count
            : _count = GetComponentsInChildren<Text>().FirstOrDefault(text => text.name == "Count");

        private Text Value => _value != null
            ? _value
            : _value = GetComponentsInChildren<Text>().FirstOrDefault(text => text.name == "Value");

        public void UpdateItemData(object data)
        {
            if (!(data is VerticalScrollItemData verticalScrollingItemData)) return;
            gameObject.SetActive(true);
            if (_data == verticalScrollingItemData) return;
            _data = verticalScrollingItemData;
            Title.text = _data.title;
            Count.text = $"Count {_data.count:00}";
            Value.text = _data.value;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    public class VerticalScrollItemData
    {
        public string title;
        public int count;
        public string value;
    }
}