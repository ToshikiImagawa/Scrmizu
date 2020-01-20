// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scrmizu
{
    [AddComponentMenu("UI/Nested Scroll Rect", 37)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class NestedScrollRect : ScrollRect
    {
        protected internal virtual bool IsNestedScroll { get; } = true;

        private bool _routeToParent;
        private IInitializePotentialDragHandler _parentInitializePotentialDragHandler;
        private IDragHandler _parentDragHandler;
        private IBeginDragHandler _parentBeginDragHandler;
        private IEndDragHandler _parentEndDragHandler;

        private IInitializePotentialDragHandler ParentInitializePotentialDragHandler
        {
            get
            {
                if (_parentInitializePotentialDragHandler != null) return _parentInitializePotentialDragHandler;
                if (transform.parent == null) return null;
                return _parentInitializePotentialDragHandler ?? (_parentInitializePotentialDragHandler =
                           transform.parent.GetComponentInParent<IInitializePotentialDragHandler>());
            }
        }

        private IDragHandler ParentDragHandler
        {
            get
            {
                if (_parentDragHandler != null) return _parentDragHandler;
                if (transform.parent == null) return null;
                return _parentDragHandler ?? (_parentDragHandler =
                           transform.parent.GetComponentInParent<IDragHandler>());
            }
        }

        private IBeginDragHandler ParentBeginDragHandler
        {
            get
            {
                if (_parentBeginDragHandler != null) return _parentBeginDragHandler;
                if (transform.parent == null) return null;
                return _parentBeginDragHandler ?? (_parentBeginDragHandler =
                           transform.parent.GetComponentInParent<IBeginDragHandler>());
            }
        }

        private IEndDragHandler ParentEndDragHandler
        {
            get
            {
                if (_parentEndDragHandler != null) return _parentEndDragHandler;
                if (transform.parent == null) return null;
                return _parentEndDragHandler ?? (_parentEndDragHandler =
                           transform.parent.GetComponentInParent<IEndDragHandler>());
            }
        }

        public sealed override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!IsNestedScroll)
            {
                base.OnInitializePotentialDrag(eventData);
                return;
            }

            ParentInitializePotentialDragHandler?.OnInitializePotentialDrag(eventData);
            base.OnInitializePotentialDrag(eventData);
        }

        public sealed override void OnDrag(PointerEventData eventData)
        {
            if (!IsNestedScroll)
            {
                base.OnDrag(eventData);
                return;
            }

            if (_routeToParent) ParentDragHandler?.OnDrag(eventData);
            else base.OnDrag(eventData);
        }

        public sealed override void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsNestedScroll)
            {
                base.OnBeginDrag(eventData);
                return;
            }

            if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
                _routeToParent = true;
            else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
                _routeToParent = true;
            else
                _routeToParent = false;

            if (_routeToParent) ParentBeginDragHandler?.OnBeginDrag(eventData);
            else base.OnBeginDrag(eventData);
        }

        public sealed override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsNestedScroll)
            {
                base.OnEndDrag(eventData);
                return;
            }

            if (_routeToParent) ParentEndDragHandler?.OnEndDrag(eventData);
            else base.OnEndDrag(eventData);
            _routeToParent = false;
        }
    }
}