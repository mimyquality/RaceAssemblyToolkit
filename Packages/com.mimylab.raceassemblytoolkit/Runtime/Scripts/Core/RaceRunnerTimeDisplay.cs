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
    using VRC.SDKBase;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race RunnerTime Display")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RaceRunnerTimeDisplay : UdonSharpBehaviour
    {
        [SerializeField]
        private RaceRunner _targetRunner;

        [Space]
        [SerializeField]
        private TMP_Text _runnerVarietyText;
        [SerializeField]
        private TMP_Text _driverNameText;
        [SerializeField]
        private TMP_Text _entriedCourseNameText;
        [SerializeField]
        private TMP_Text _lapCountText;
        [SerializeField]
        private TMP_Text _currentTimeText;
        [SerializeField]
        private TMP_Text _lastSectionTimeText;
        [SerializeField]
        private TMP_Text _lastSplitTimeText;
        [SerializeField]
        private TMP_Text _lastLapTimeText;
        [SerializeField]
        private string _timeFormat = "hh\\:mm\\'ss\\\"fff";

        private string _runnerVariety = "";
        public string RunnerVariety
        {
            get => _runnerVariety;
            set
            {
                if (_runnerVarietyText && _runnerVariety != value)
                {
                    _runnerVarietyText.text = value;
                }
                _runnerVariety = value;
            }
        }

        private string _driverName = "";
        public string DriverName
        {
            get => _driverName;
            set
            {
                if (_driverNameText && _driverName != value)
                {
                    _driverNameText.text = value;
                }
                _driverName = value;
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

        private int _numberOfLaps;
        public int NumberOfLaps
        {
            get => _numberOfLaps;
            set
            {
                if (_numberOfLaps != value)
                {
                    SetLapCountText(_currentLap, value);
                }
                _numberOfLaps = value;
            }
        }

        private int _currentLap;
        public int CurrentLap
        {
            get => _currentLap;
            set
            {
                if (_currentLap != value)
                {
                    SetLapCountText(value, _numberOfLaps);
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

        private TimeSpan _lastSectionTime;
        public TimeSpan LastSectionTime
        {
            get => _lastSectionTime;
            set
            {
                if (_lastSectionTimeText && _lastSectionTime != value)
                {
                    _lastSectionTimeText.text = value.ToString(_timeFormat);
                }
                _lastSectionTime = value;
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

        private void Update()
        {
            if (!_targetRunner) { return; }

            RunnerVariety = _targetRunner.variety;

            var driver = _targetRunner.GetDriver();
            DriverName = Utilities.IsValid(driver) ? driver.displayName : "";

            var course = _targetRunner.EntriedCourse;
            EntriedCourseName = course ? course.courseName : "";

            NumberOfLaps = course.numberOfLaps;
            CurrentLap = _targetRunner.LatestLap;
            CurrentTime = _targetRunner._GetCurrentTime();  // 動いてない
            LastSectionTime = _targetRunner.LatestSectionTime;
            LastSplitTime = _targetRunner.LatestSplitTime;
            LastLapTime = _targetRunner.LatestLapTime;  // 動いてない
        }

        private void SetLapCountText(int current, int count)
        {
            if (_lapCountText)
            {
                _lapCountText.text = $"{current} / {count}";
            }
        }
    }
}
