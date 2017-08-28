using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace VIS
{
    public class VariableInfiniteScroll : ScrollRect
    {
        /// <summary>
        /// The contentItem that can be instantiated in the scroll area.
        /// </summary>
        public RectTransform contentItem { get; set; }
        /// <summary>
        /// Count to be instantiated.
        /// </summary>
        public int instantateItemCount { get; set; }
    }
}
