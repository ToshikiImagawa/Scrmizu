// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Scrmizu.Editor
{
    [CustomEditor(typeof(InfiniteScrollRect), true)]
    [CanEditMultipleObjects]
    public class InfiniteScrollEditor : UnityEditor.Editor
    {
        private const string HError =
            "For this visibility mode, the Viewport property and the Horizontal Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";

        private const string VError =
            "For this visibility mode, the Viewport property and the Vertical Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";

        private SerializedProperty _direction;
        private SerializedProperty _isReverse;
        private SerializedProperty _itemBase;
        private SerializedProperty _defaultItemSize;
        private SerializedProperty _itemInterval;
        private SerializedProperty _instantiatedItemCount;
        
        private SerializedProperty _content;
        private SerializedProperty _horizontal;
        private SerializedProperty _vertical;
        private SerializedProperty _movementType;
        private SerializedProperty _elasticity;
        private SerializedProperty _inertia;
        private SerializedProperty _decelerationRate;
        private SerializedProperty _scrollSensitivity;
        private SerializedProperty _viewport;
        private SerializedProperty _horizontalScrollbar;
        private SerializedProperty _verticalScrollbar;
        private SerializedProperty _horizontalScrollbarVisibility;
        private SerializedProperty _verticalScrollbarVisibility;
        private SerializedProperty _horizontalScrollbarSpacing;
        private SerializedProperty _verticalScrollbarSpacing;
        private SerializedProperty _onValueChanged;
        private AnimBool _showElasticity;
        private AnimBool _showDecelerationRate;
        private bool _viewportIsNotChild;
        private bool _hScrollbarIsNotChild;
        private bool _vScrollbarIsNotChild;

        protected void OnEnable()
        {
            _direction = serializedObject.FindProperty("direction");
            _isReverse = serializedObject.FindProperty("isReverse");
            _itemBase = serializedObject.FindProperty("itemBase");
            _defaultItemSize = serializedObject.FindProperty("defaultItemSize");
            _itemInterval = serializedObject.FindProperty("itemInterval");
            _instantiatedItemCount = serializedObject.FindProperty("instantiatedItemCount");

            _content = serializedObject.FindProperty("m_Content");
            _horizontal = serializedObject.FindProperty("m_Horizontal");
            _vertical = serializedObject.FindProperty("m_Vertical");
            _movementType = serializedObject.FindProperty("m_MovementType");
            _elasticity = serializedObject.FindProperty("m_Elasticity");
            _inertia = serializedObject.FindProperty("m_Inertia");
            _decelerationRate = serializedObject.FindProperty("m_DecelerationRate");
            _scrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
            _viewport = serializedObject.FindProperty("m_Viewport");
            _horizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
            _verticalScrollbar = serializedObject.FindProperty("m_VerticalScrollbar");
            _horizontalScrollbarVisibility = serializedObject.FindProperty("m_HorizontalScrollbarVisibility");
            _verticalScrollbarVisibility = serializedObject.FindProperty("m_VerticalScrollbarVisibility");
            _horizontalScrollbarSpacing = serializedObject.FindProperty("m_HorizontalScrollbarSpacing");
            _verticalScrollbarSpacing = serializedObject.FindProperty("m_VerticalScrollbarSpacing");
            _onValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            _showElasticity = new AnimBool(Repaint);
            _showDecelerationRate = new AnimBool(Repaint);
            SetAnimBooleans(true);
        }

        protected virtual void OnDisable()
        {
            _showElasticity.valueChanged.RemoveListener(Repaint);
            _showDecelerationRate.valueChanged.RemoveListener(Repaint);
        }

        private void SetAnimBooleans(bool instant)
        {
            SetAnimBool(_showElasticity,
                !_movementType.hasMultipleDifferentValues && _movementType.enumValueIndex == 1, instant);
            SetAnimBool(_showDecelerationRate,
                !_inertia.hasMultipleDifferentValues && _inertia.boolValue, instant);
        }

        private static void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
                a.value = value;
            else
                a.target = value;
        }

        private void CalculateCachedValues()
        {
            _viewportIsNotChild = false;
            _hScrollbarIsNotChild = false;
            _vScrollbarIsNotChild = false;
            if (targets.Length != 1)
                return;
            var transform = ((Component) target).transform;
            if (_viewport.objectReferenceValue == null ||
                ((Component) _viewport.objectReferenceValue).transform.parent != transform)
                _viewportIsNotChild = true;
            if (_horizontalScrollbar.objectReferenceValue == null ||
                ((Component) _horizontalScrollbar.objectReferenceValue).transform.parent !=
                transform)
                _hScrollbarIsNotChild = true;
            if (_verticalScrollbar.objectReferenceValue == null ||
                ((Component) _verticalScrollbar.objectReferenceValue).transform.parent !=
                transform)
                _vScrollbarIsNotChild = true;
        }

        public override void OnInspectorGUI()
        {
            SetAnimBooleans(false);
            serializedObject.Update();
            CalculateCachedValues();

            switch (_direction.enumValueIndex)
            {
                case (int) Direction.Vertical:
                    _horizontal.boolValue = false;
                    _vertical.boolValue = true;
                    break;
                case (int) Direction.Horizontal:
                    _horizontal.boolValue = true;
                    _vertical.boolValue = false;
                    break;
                default:
                    _horizontal.boolValue = false;
                    _vertical.boolValue = false;
                    break;
            }

            EditorGUILayout.PropertyField(_direction);
            EditorGUILayout.PropertyField(_isReverse);

            var itemBaseObject = _itemBase.objectReferenceValue as MonoBehaviour;
            if (itemBaseObject != null)
            {
                var infiniteScrollItem = itemBaseObject.GetComponent<IInfiniteScrollItem>();
                if (infiniteScrollItem == null)
                {
                    _itemBase.objectReferenceValue = null;
                }
            }
            else
            {
                _itemBase.objectReferenceValue = null;
            }

            EditorGUILayout.PropertyField(_itemBase);
            EditorGUILayout.PropertyField(_instantiatedItemCount);

            switch (_direction.enumValueIndex)
            {
                case (int) Direction.Vertical:
                    _defaultItemSize.floatValue =
                        EditorGUILayout.FloatField("Default Height", _defaultItemSize.floatValue);
                    break;
                case (int) Direction.Horizontal:
                    _defaultItemSize.floatValue =
                        EditorGUILayout.FloatField("Default Width", _defaultItemSize.floatValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _itemInterval.floatValue =
                EditorGUILayout.FloatField("Interval", _itemInterval.floatValue);

            EditorGUILayout.PropertyField(_content);
            EditorGUILayout.PropertyField(_movementType);
            _movementType.enumValueIndex = 1;

            if (EditorGUILayout.BeginFadeGroup(_showElasticity.faded))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_elasticity);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.PropertyField(_inertia);
            if (EditorGUILayout.BeginFadeGroup(_showDecelerationRate.faded))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_decelerationRate);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.PropertyField(_scrollSensitivity);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_viewport);
            if (_horizontal.boolValue)
            {
                EditorGUILayout.PropertyField(_horizontalScrollbar);
                if ((bool) _horizontalScrollbar.objectReferenceValue &&
                    !_horizontalScrollbar.hasMultipleDifferentValues)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_horizontalScrollbarVisibility,
                        EditorGUIUtility.TrTextContent("Visibility", null, (Texture) null));
                    if (_horizontalScrollbarVisibility.enumValueIndex == 2 &&
                        !_horizontalScrollbarVisibility.hasMultipleDifferentValues)
                    {
                        if (_viewportIsNotChild || _hScrollbarIsNotChild)
                            EditorGUILayout.HelpBox(HError, MessageType.Error);
                        EditorGUILayout.PropertyField(_horizontalScrollbarSpacing,
                            EditorGUIUtility.TrTextContent("Spacing", null, (Texture) null));
                    }

                    --EditorGUI.indentLevel;
                }
            }

            if (_vertical.boolValue)
            {
                EditorGUILayout.PropertyField(_verticalScrollbar);
                if ((bool) _verticalScrollbar.objectReferenceValue &&
                    !_verticalScrollbar.hasMultipleDifferentValues)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_verticalScrollbarVisibility,
                        EditorGUIUtility.TrTextContent("Visibility", null, (Texture) null));
                    if (_verticalScrollbarVisibility.enumValueIndex == 2 &&
                        !_verticalScrollbarVisibility.hasMultipleDifferentValues)
                    {
                        if (_viewportIsNotChild || _vScrollbarIsNotChild)
                            EditorGUILayout.HelpBox(VError, MessageType.Error);
                        EditorGUILayout.PropertyField(_verticalScrollbarSpacing,
                            EditorGUIUtility.TrTextContent("Spacing", null, (Texture) null));
                    }

                    --EditorGUI.indentLevel;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_onValueChanged);
            serializedObject.ApplyModifiedProperties();
        }
    }
}