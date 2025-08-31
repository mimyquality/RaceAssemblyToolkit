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
        private TMP_Text _maxLapsText;
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
                if (_maxLapsText) { _maxLapsText.text = value.ToString("D2"); }
                _maxLaps = value;
            }
        }

        private int _lap;
        public int Lap
        {
            get => _lap;
            set
            {
                if (_lapText) { _lapText.text = value.ToString("D2"); }
                _lap = value;
            }
        }

        private TimeSpan _lapTime;
        public TimeSpan LapTime
        {
            get => _lapTime;
            set
            {
                if (_lapTimeText) { _lapTimeText.text = value.ToString(_timeFormat); }
                _lapTime = value;
            }
        }

        private TimeSpan _splitTime;
        public TimeSpan SplitTime
        {
            get => _splitTime;
            set
            {
                if (_splitTimeText) { _splitTimeText.text = value.ToString(_timeFormat); }
                _splitTime = value;
            }
        }

        private TimeSpan _totalTime;
        public TimeSpan TotalTime
        {
            get => _totalTime;
            set
            {
                if (_totalTimeText) { _totalTimeText.text = value.ToString(_timeFormat); }
                _totalTime = value;
            }
        }
    }
}
