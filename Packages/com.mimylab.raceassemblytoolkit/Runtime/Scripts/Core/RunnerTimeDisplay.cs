﻿/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using System;
    using UdonSharp;
    using UnityEngine;
    using TMPro;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/RunnerTime Display")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RunnerTimeDisplay : UdonSharpBehaviour
    {
        [SerializeField]
        private Runner _targetRunner;

        [Space]
        [SerializeField]
        private TMP_Text _runnerNameText;
        [SerializeField]
        private TMP_Text _entriedCourseNameText;
        [SerializeField]
        private TMP_Text _lapCountText;
        [SerializeField]
        private TMP_Text _currentTimeText;
        [SerializeField]
        private TMP_Text _lastLapTimeText;
        [SerializeField]
        private TMP_Text _lastSplitTimeText;
        [SerializeField]
        private string _timeFormat = "hh\\:mm\\'ss\\\"fff";

        private string _lapCountString = "0/0";

        private string _runnerName = "";
        public string RunnerName
        {
            get => _runnerName;
            set
            {
                if (_runnerNameText && _runnerName != value)
                {
                    _runnerNameText.text = value;
                }
                _runnerName = value;
            }
        }

        private string _entriedCourseName = "";
        public string EntriedCourseName
        {
            get => _entriedCourseName;
            set
            {
                if (_entriedCourseNameText && _entriedCourseName != value)
                {
                    _entriedCourseNameText.text = value;
                }
                _entriedCourseName = value;
            }
        }

        private int _lapCount;
        public int LapCount
        {
            get => _lapCount;
            set
            {
                if (_lapCountText && _lapCount != value)
                {
                    SetLapCountText(_currentLap, value);
                }
                _lapCount = value;
            }
        }

        private int _currentLap;
        public int CurrentLap
        {
            get => _currentLap;
            set
            {
                if (_lapCountText && _currentLap != value)
                {
                    SetLapCountText(value, _lapCount);
                }
                _currentLap = value;
            }
        }

        private TimeSpan _currentTime;
        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_currentTimeText && _currentTime != value)
                {
                    _currentTimeText.text = value.ToString(_timeFormat);
                }
                _currentTime = value;
            }
        }

        private TimeSpan _lastLapTime;
        public TimeSpan LastLapTime
        {
            get => _lastLapTime;
            set
            {
                if (_lastLapTimeText && _lastLapTime != value)
                {
                    _lastLapTimeText.text = value.ToString(_timeFormat);
                }
                _lastLapTime = value;
            }
        }

        private TimeSpan _lastSplitTime;
        public TimeSpan LastSplitTime
        {
            get => _lastSplitTime;
            set
            {
                if (_lastSplitTimeText && _lastSplitTime != value)
                {
                    _lastSplitTimeText.text = value.ToString(_timeFormat);
                }
                _lastSplitTime = value;
            }
        }

        private void Start()
        {
            _targetRunner.display = this;
        }

        private void SetLapCountText(int current, int count)
        {
            _lapCountText.text = $"{current} / {count}";
        }
    }
}
