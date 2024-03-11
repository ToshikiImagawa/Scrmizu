[![license](https://img.shields.io/github/license/ToshikiImagawa/Variable-infinite-scroll?style=flat-square)](https://github.com/ToshikiImagawa/Variable-infinite-scroll/blob/master/LICENSE.md)
[![release](https://img.shields.io/github/release/ToshikiImagawa/Variable-infinite-scroll?style=flat-square)](https://github.com/ToshikiImagawa/Variable-infinite-scroll/releases)

# What is Scrmizu
Scrmizu is variable infinite scroll and extended UnityEngine.UI.ScrollRect for Unity UGUI.

# Required
- Unity 2020.3.45f1 or later.
- Scripting Runtime Version 4.6 Eq.

# Install
1. Download Scrmizu.unitypackage [here](https://github.com/ToshikiImagawa/Variable-infinite-scroll/releases).
1. Import the contents of the Scrmizu folder to the Packages or Assets folder.
1. Optionally import Scrmizu.Sample.unitypackage.

# Quick start
## Infinite Scroll View
To create a Infinite Scroll View in the Unity Editor, go to GameObject → UI → Infinite Scroll View.

<img src="https://user-images.githubusercontent.com/6396938/72912227-aa445500-3d7e-11ea-8c7b-59da594e0a0c.png" width="300px"/>

Prepare ScrollItem prefab to be repeatedly displayed by infinite scroll.

<img src="https://user-images.githubusercontent.com/6396938/72912979-ce546600-3d7f-11ea-8107-38b2b826b9e6.png" width="300px"/>

```c#:SimpleInfiniteScrollItem.cs
public class SimpleInfiniteScrollItem : MonoBehaviour, IInfiniteScrollItem
{
    /// <summary>
    /// ScrollItem enters display area and updates display item data.
    /// </summary>
    /// <param name="data"></param>
    public void UpdateItemData(object data)
    {
        if (!(data is float width)) return;
        gameObject.SetActive(true);
        if (!(gameObject.transform is RectTransform rectTransform)) return;
        rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
    }

    /// <summary>
    /// Hide ScrollItem because it has left the display area.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
```

Call InfiniteScrollRect.SetItemData to set the item data.

```c#:SimpleInfiniteScrollController.cs
[RequireComponent(typeof(InfiniteScrollRect))]
public class SimpleInfiniteScrollController : MonoBehaviour
{
    private InfiniteScrollRect _infiniteScrollRect;

    private InfiniteScrollRect InfiniteScrollRect => _infiniteScrollRect != null
        ? _infiniteScrollRect : _infiniteScrollRect = GetComponent<InfiniteScrollRect>();

    private void Awake()
    {
        InfiniteScrollRect.SetItemData(new object[]
        {
            200f, 
            300f, 
            400f, 
            500f, 
            600f, 
            700f, 
            800f, 
            900f, 
            1000f
        });
    }
}
```

By scrolling, you can confirm that the width of the scroll item is updated for each set data (width).

## Nested Scroll View
To create a Nested Scroll View in the Unity Editor, go to GameObject → UI → Nested Scroll View.
Put it under the Scroll View container.

<img src="https://user-images.githubusercontent.com/6396938/72831133-8bce5300-3cc5-11ea-8cae-5cf5447fcbfa.png" width="300px"/>

When scrolling in the non-scroll direction of NestedScrollView, scroll event is sent to the parent ScrollView.

## Paged Scroll View
To create a Paged Scroll View in the Unity Editor, go to GameObject → UI → Paged Scroll View.
Create multiple page content items under content.

<img src="https://user-images.githubusercontent.com/6396938/72828751-ab16b180-3cc0-11ea-9783-3efffd775ae9.png" width="300px"/>

One item is displayed in the scroll direction in the view area.
Pauses the page each time scroll.