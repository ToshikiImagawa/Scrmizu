using System;
using UnityEngine;

namespace Scrmizu
{
    public interface IInfiniteScrollItem
    {
        void UpdateItemData(object data);
        void Hide();
    }
}