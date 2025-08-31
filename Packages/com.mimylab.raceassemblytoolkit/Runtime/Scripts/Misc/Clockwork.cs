/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using System;
    using TMPro;
    using UdonSharp;
    using UnityEngine;

    [Icon(ComponentIconPath.Clock)]
    [AddComponentMenu("Race Assembly Toolkit/Misc/Clockwork")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Clockwork : UdonSharpBehaviour
    {
        [SerializeField]
        private TMP_Text _clockText = null;
        [SerializeField]
        private string _timeFormat = "f";

        private void Reset()
        {
            _clockText = this.GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (!_clockText) { return; }

            _UpdateClock();
        }

        public void _UpdateClock()
        {
            if (!(this.gameObject.activeInHierarchy && this.enabled)) { return; }
            if (!_clockText) { return; }

            var now = DateTime.Now;
            _clockText.text = now.ToString(_timeFormat);

            SendCustomEventDelayedSeconds(nameof(_UpdateClock), 60 - now.Second);
        }
    }
}
