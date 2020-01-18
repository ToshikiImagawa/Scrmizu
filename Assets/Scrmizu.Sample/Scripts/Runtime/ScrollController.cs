// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrmizu.Sample
{
    [RequireComponent(typeof(InfiniteScrollRect))]
    public class ScrollController : MonoBehaviour
    {
        private InfiniteScrollRect _infiniteScroll;

        private InfiniteScrollRect InfiniteScroll => _infiniteScroll != null
            ? _infiniteScroll
            : _infiniteScroll = GetComponent<InfiniteScrollRect>();

        private int index = 0;

        private void Awake()
        {
            InfiniteScroll.SetItemData(new[]
            {
                new ScrollItemData
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

            var list = new List<ScrollItemData>();
            for (var i = 0; i < 100; i++)
            {
                index++;
                list.Add(new ScrollItemData
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
            InfiniteScroll.AddItemData(new ScrollItemData
            {
                title = $" タイトル {index:00}",
                count = index,
                value = string.Join("\n", Enumerable.Repeat($"{index}", index).ToArray())
            });
        }

        [ContextMenu("RemoveAtItem")]
        public void RemoveAtItem()
        {
            InfiniteScroll.RemoveAtItemData(5);
        }

        [ContextMenu("InsertItem")]
        public void InsertItem()
        {
            index++;
            InfiniteScroll.InsertItemData(0,new ScrollItemData
            {
                title = $" タイトル {index:00}",
                count = index,
                value = string.Join("\n", Enumerable.Repeat($"{index}", index).ToArray())
            });
        }

        [ContextMenu("AddRange")]
        public void AddRange()
        {
            var list = new List<ScrollItemData>();
            for (var i = 0; i < 25; i++)
            {
                index++;
                list.Add(new ScrollItemData
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

        [ContextMenu("InsertRange")]
        public void InsertRange()
        {
            var list = new List<ScrollItemData>();
            for (var i = 0; i < 25; i++)
            {
                index++;
                list.Add(new ScrollItemData
                {
                    title = $" タイトル {index:00}",
                    count = index,
                    value = string.Join("\n", Enumerable.Repeat($"{index}", index).ToArray())
                });
            }
            InfiniteScroll.InsertRangeItemData(0, list);
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            InfiniteScroll.ClearItemData();
        }

        [ContextMenu("MovePositionAt")]
        public void MovePositionAt()
        {
            InfiniteScroll.MovePositionAt(5);
        }
    }
}