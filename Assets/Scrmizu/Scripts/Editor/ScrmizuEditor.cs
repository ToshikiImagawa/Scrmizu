// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using Scrmizu.Editor;
using UnityEditor;

namespace Scrmizu.Editor
{
    public class ScrmizuEditor
    {
        [MenuItem("GameObject/UI/Infinite Scroll View", priority = 2063)]
        public static void CreateInfiniteScrollView()
        {
            ScrmizuEditorUtil.CreateInfiniteScrollView(Selection.gameObjects);
        }

        [MenuItem("GameObject/UI/Nested Scroll View", priority = 2063)]
        public static void CreateNestedScrollView()
        {
            ScrmizuEditorUtil.CreateNestedScrollView(Selection.gameObjects);
        }

        [MenuItem("GameObject/UI/Paged Scroll View", priority = 2063)]
        public static void CreatePagedScrollView()
        {
            ScrmizuEditorUtil.CreatePagedScrollView(Selection.gameObjects);
        }
    }
}