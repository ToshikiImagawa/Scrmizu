// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scrmizu.Sample
{
    [RequireComponent(typeof(RectTransform))]
    public class VerticalScrollItem : MonoBehaviour, IInfiniteScrollItem
    {
        private ScrollItemData _data;

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
            if (!(data is ScrollItemData scrollingItemData)) return;
            if (_data != scrollingItemData)
            {
                _data = scrollingItemData;
                Title.text = _data.title;
                Count.text = $"Count {_data.count:00}";
                Value.text = _data.value;
            }
            Canvas.ForceUpdateCanvases();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}