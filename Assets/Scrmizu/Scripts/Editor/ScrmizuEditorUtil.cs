// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Scrmizu.Editor
{
    internal static class ScrmizuEditorUtil
    {
        public static InfiniteScrollRect CreateInfiniteScrollView(GameObject[] gameObjects)
        {
            var createPoint = GetCreatePointOfUgui(gameObjects);
            var infiniteScrollRectObj = Object.Instantiate(
                LoadAssetAtPath<GameObject>("Prefabs/Infinite Scroll View.prefab"),
                Vector3.zero, Quaternion.identity);
            infiniteScrollRectObj.name = "Infinite Scroll View";
            infiniteScrollRectObj.transform.SetParent(createPoint, false);
            return infiniteScrollRectObj.GetComponent<InfiniteScrollRect>();
        }

        public static NestedScrollRect CreateNestedScrollView(GameObject[] gameObjects)
        {
            var createPoint = GetCreatePointOfUgui(gameObjects);
            var nestedScrollRect = Object.Instantiate(
                LoadAssetAtPath<GameObject>("Prefabs/Nested Scroll View.prefab"),
                Vector3.zero, Quaternion.identity);
            nestedScrollRect.name = "Nested Scroll View";
            nestedScrollRect.transform.SetParent(createPoint, false);
            return nestedScrollRect.GetComponent<NestedScrollRect>();
        }

        public static PagedScrollRect CreatePagedScrollView(GameObject[] gameObjects)
        {
            var createPoint = GetCreatePointOfUgui(gameObjects);
            var nestedScrollRect = Object.Instantiate(
                LoadAssetAtPath<GameObject>("Prefabs/Paged Scroll View.prefab"),
                Vector3.zero, Quaternion.identity);
            nestedScrollRect.name = "Paged Scroll View";
            nestedScrollRect.transform.SetParent(createPoint, false);
            return nestedScrollRect.GetComponent<PagedScrollRect>();
        }

        private static Canvas CanvasFindOrCreate(GameObject[] gameObjects)
        {
            var canvas = GetComponentInChildrenOrAll<Canvas>(gameObjects);
            if (canvas != null)
            {
                return canvas;
            }

            var canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            EventSystemFindOrCreate(gameObjects);
            return canvas;
        }

        private static EventSystem EventSystemFindOrCreate(GameObject[] gameObjects)
        {
            var eventSystem = GetComponentInChildrenOrAll<EventSystem>(gameObjects);
            if (eventSystem != null)
            {
                return eventSystem;
            }

            var eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            return eventSystem;
        }

        private static RectTransform CreateContent(Transform transform)
        {
            var content = new GameObject("Content").AddComponent<RectTransform>();
            content.SetParent(transform, false);
            return content;
        }

        private static RectTransform CreateSlidingArea(Transform transform)
        {
            var slidingArea = new GameObject("Sliding Area").AddComponent<RectTransform>();
            slidingArea.SetParent(transform, false);
            CreateHandle(slidingArea);
            return slidingArea;
        }

        private static GameObject CreateHandle(Transform transform)
        {
            var handle = new GameObject("Handle");
            handle.transform.SetParent(transform, false);
            handle.AddComponent<Image>();
            return handle;
        }

        private static Transform GetCreatePointOfUgui(GameObject[] gameObjects)
        {
            if (gameObjects == null) return CanvasFindOrCreate(null).transform;
            foreach (var gameObject in gameObjects)
            {
                var canvas = gameObject.GetComponentInParent<Canvas>();
                if (canvas != null) return canvas.transform;
            }

            return CanvasFindOrCreate(gameObjects).transform;
        }

        private static GameObject[] FindRootObject()
        {
            return Array.FindAll(Object.FindObjectsOfType<GameObject>(), (item) => item.transform.parent == null)
                .ToArray();
        }

        private static TComponent GetComponentInChildrenAll<TComponent>()
        {
            foreach (var gameObject in FindRootObject())
            {
                var component = gameObject.GetComponentInChildren<TComponent>();
                if (component != null)
                {
                    return component;
                }
            }

            return default;
        }

        private static TComponent GetComponentInChildrenOrAll<TComponent>(GameObject[] gameObjects)
        {
            TComponent component;
            if (gameObjects != null)
            {
                foreach (var gameObject in gameObjects)
                {
                    if (gameObject == null) continue;
                    component = gameObject.GetComponentInChildren<TComponent>();
                    if (component != null)
                    {
                        return component;
                    }
                }
            }

            component = GetComponentInChildrenAll<TComponent>();
            return component != null ? component : default;
        }

        private static T LoadAssetAtPath<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Path is empty.");
            var obj = AssetDatabase.LoadAssetAtPath<T>($"Assets/Scrmizu/Editor Resources/{path}");
            if (obj != null)
            {
                return obj;
            }

            obj = AssetDatabase.LoadAssetAtPath<T>($"Packages/Scrmizu/Editor Resources/{path}");
            if (obj != null)
            {
                return obj;
            }

            var extension = Path.GetExtension(path);
            var resourcesPath = string.IsNullOrEmpty(extension) ? path : path.Replace(extension, string.Empty);
            obj = EditorResources.Load<T>(resourcesPath);
            if (obj != null)
            {
                return obj;
            }

            throw new Exception("Problems with the folder structure.");
        }
    }
}