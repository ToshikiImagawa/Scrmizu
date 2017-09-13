using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Antibody
{
    public class InfiniteScroll : ScrollRect, IScrollHandler
    {
        /// <summary>
        /// The contentItem that can be instantiated in the scroll area.
        /// </summary>
        [SerializeField] private RectTransform _itemBase;
        /// <summary>
        /// Decide whether to play automatically.
        /// </summary>
        [SerializeField] private bool _autoStart = false;
        /// <summary>
        /// Count to be instantiated.
        /// </summary>
        [SerializeField, Range(0, 30)] int _instantateItemCount = 9;

        [NonSerialized] public List<RectTransform> m_itemList = new List<RectTransform>();

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnScrollMove);
        }

        private void OnScrollMove(Vector2 call)
        {

        }
    }
}
