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
                    title = "Title あ",
                    count = 1,
                    value = "あああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ"
                },
                new VerticalScrollItemData
                {
                    title = "Title い",
                    count = 2,
                    value = "いいいいいいいいい\n" +
                            "いいいいいいいいいい\n" +
                            "いいいいいいいいいい\n" +
                            "いいいいいいいいいい\n" +
                            "いいいいいいい"
                },
                new VerticalScrollItemData
                {
                    title = "Title う",
                    count = 3,
                    value = "ううううううううう\n" +
                            "うううううううううう\n" +
                            "うううううううううう\n" +
                            "うううううううううう\n" +
                            "ううううううう"
                },
                new VerticalScrollItemData
                {
                    title = "Title え",
                    count = 4,
                    value = "あああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ\n" +
                            "ああああああああああ"
                },
                new VerticalScrollItemData
                {
                    title = "Title お",
                    count = 5,
                    value = "いいいいいいいいい\n" +
                            "いいいいいいいいいい\n" +
                            "いいいいいいいいいい\n" +
                            "いいいいいいいいいい\n" +
                            "いいいいいいい"
                },
                new VerticalScrollItemData
                {
                    title = "Title か",
                    count = 6,
                    value = "ううううううううう\n" +
                            "うううううううううう\n" +
                            "うううううううううう\n" +
                            "うううううううううう\n" +
                            "ううううううう"
                },
            });
        }

        [ContextMenu("AddItem")]
        public void AddItem()
        {
            index++;
        }
    }
}