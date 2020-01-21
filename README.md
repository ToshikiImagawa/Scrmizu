[![license](https://img.shields.io/github/license/ToshikiImagawa/Variable-infinite-scroll?style=flat-square)](https://github.com/ToshikiImagawa/Variable-infinite-scroll/blob/master/LICENSE.md)
[![release](https://img.shields.io/github/release/ToshikiImagawa/Variable-infinite-scroll?style=flat-square)](https://github.com/ToshikiImagawa/Variable-infinite-scroll/releases)

# What is Scrmizu
Scrmizu is variable infinite scroll and extended UnityEngine.UI.ScrollRect for Unity UGUI.

# Required
- Unity 2018.x or later.
- Scripting Runtime Version 4.6 Eq.

# Install
1. Download Scrmizu.unitypackage [here](https://github.com/ToshikiImagawa/Variable-infinite-scroll/releases).
1. Import the contents of the Scrmizu folder to the Packages or Assets folder.
1. Optionally import Scrmizu.Sample.unitypackage.

# Quick start
## Infinite Scroll View
To create a Infinite Scroll View in the Unity Editor, go to GameObject → UI → Infinite Scroll View.

## Nested Scroll View
To create a Nested Scroll View in the Unity Editor, go to GameObject → UI → Nested Scroll View.
Put it under the Scroll View container.

![NestedScrollRect-HierarchyWindow](https://user-images.githubusercontent.com/6396938/72831133-8bce5300-3cc5-11ea-8cae-5cf5447fcbfa.png)

When scrolling in the non-scroll direction of NestedScrollView, scroll event is sent to the parent ScrollView.

## Paged Scroll View
To create a Paged Scroll View in the Unity Editor, go to GameObject → UI → Paged Scroll View.
Create multiple page content items under content.

![PagedScrollRect-HierarchyWindow](https://user-images.githubusercontent.com/6396938/72828751-ab16b180-3cc0-11ea-9783-3efffd775ae9.png)

One item is displayed in the scroll direction in the view area.
Pauses the page each time scroll.