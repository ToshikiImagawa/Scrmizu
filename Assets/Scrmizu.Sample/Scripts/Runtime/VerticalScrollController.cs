using System.Collections.Generic;
using System.Linq;
using Scrmizu;
using UnityEngine;

namespace Scrmizu_Sample
{
    [RequireComponent(typeof(InfiniteScroll))]
    public class VerticalScrollController : MonoBehaviour
    {
        private InfiniteScroll _infiniteScroll;

        private InfiniteScroll InfiniteScroll => _infiniteScroll != null
            ? _infiniteScroll
            : _infiniteScroll = GetComponent<InfiniteScroll>();

        private int index = 0;

        private void Awake()
        {
            InfiniteScroll.SetItemData(new[]
            {
                new VerticalScrollItemData
                {
                    title = "Title 00",
                    count = 0,
                    value = "あああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ"
                }
            });

            var list = new List<VerticalScrollItemData>();
            for (var i = 0; i < 100; i++)
            {
                index++;
                list.Add(new VerticalScrollItemData
                {
                    title = $" タイトル {index:00}",
                    count = index,
                    value = string.Join("\n", Enumerable.Repeat($"{index}", index).ToArray())
                });
            }
            InfiniteScroll.AddRangeItemData(list);
        }

        [ContextMenu("AddItem")]
        public void AddItem()
        {
            index++;
            InfiniteScroll.AddItemData(new VerticalScrollItemData
            {
                title = $" タイトル {index:00}",
                count = index,
                value = string.Join("\n", Enumerable.Repeat($"{index}", index).ToArray())
            });
        }

        [ContextMenu("AddRange")]
        public void AddRange()
        {
            var list = new List<VerticalScrollItemData>();
            for (var i = 0; i < 25; i++)
            {
                index++;
                list.Add(new VerticalScrollItemData
                {
                    title = $" タイトル {index:00}",
                    count = index,
                    value = string.Join("\n", Enumerable.Repeat($"{index}", index).ToArray())
                });
            }
            InfiniteScroll.AddRangeItemData(list);
        }


        [ContextMenu("RemoveRange")]
        public void RemoveRange()
        {
            InfiniteScroll.RemoveRangeItemData(0, 25);
        }
    }
}