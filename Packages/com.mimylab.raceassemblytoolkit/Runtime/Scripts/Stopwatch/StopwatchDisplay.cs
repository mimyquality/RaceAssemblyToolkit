/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    using TMPro;
    using System;

    [Icon(ComponentIconPath.Stopwatch)]
    [AddComponentMenu("Race Assembly Toolkit/Stopwatch/Stopwatch Display")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StopwatchDisplay : UdonSharpBehaviour
    {
        [SerializeField]
        private TMP_Text _lapText;
        [SerializeField]
        private TMP_Text _lapTimeText;
        [SerializeField]
        private TMP_Text _splitTimeText;
        [SerializeField]
        private TMP_Text _totalTimeText;
        [SerializeField]
        private string _timeFormat = "hh\\:mm\\'ss\\\"fff";

        private int _maxLaps;
        public int MaxLaps
        {
            get => _maxLaps;
            set
            {
                _maxLaps = value;
                SetLapText();
            }
        }

        private int _lap;
        public int Lap
        {
            get => _lap;
            set
            {
                _lap = value;
                SetLapText();
            }
        }

        private void SetLapText()
        {
            if (_lapText)
            {
                _lapText.text = $"{_lap:D2}/{_maxLaps:D2}";
            }
        }

        private TimeSpan _lapTime;
        public TimeSpan LapTime
        {
            get => _lapTime;
            set
            {
                _lapTime = value;
                if (_lapTimeText) { _lapTimeText.text = value.ToString(_timeFormat); }
            }
        }

        private TimeSpan _splitTime;
        public TimeSpan SplitTime
        {
            get => _splitTime;
            set
            {
                _splitTime = value;
                if (_splitTimeText) { _splitTimeText.text = value.ToString(_timeFormat); }
            }
        }

        private TimeSpan _totalTime;
        public TimeSpan TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                if (_totalTimeText) { _totalTimeText.text = value.ToString(_timeFormat); }
            }
        }
    }
}
