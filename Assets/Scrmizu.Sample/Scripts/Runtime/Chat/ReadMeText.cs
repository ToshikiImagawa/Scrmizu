// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scrmizu.Sample
{
    [RequireComponent(typeof(Text))]
    public class ReadMeText : MonoBehaviour
    {
        [SerializeField] private TextAsset readMe;
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