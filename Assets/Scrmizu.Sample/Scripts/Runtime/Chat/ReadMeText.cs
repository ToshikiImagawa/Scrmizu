// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Scrmizu.Sample.Chat
{
    [RequireComponent(typeof(Text))]
    public class ReadMeText : MonoBehaviour
    {
        [SerializeField] private TextAsset readMe = default;
        private Text _text;
        private Text Text => _text != null ? _text : _text = GetComponent<Text>();

        private void Awake()
        {
            if (readMe != null)
            {
                Text.text = readMe.text;
            }
        }
    }
}