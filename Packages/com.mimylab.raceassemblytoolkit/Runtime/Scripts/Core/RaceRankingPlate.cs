/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using System;
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using TMPro;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Ranking Plate")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RaceRankingPlate : UdonSharpBehaviour
    {
        [SerializeField]
        private TMP_Text _courseNameText;
        [SerializeField]
        private TMP_Text _numberOfLapsText;
        [SerializeField]
        private TMP_Text _driverNameText;
        [SerializeField]
        private TMP_Text _runnerVarietyText;
        [SerializeField]
        private TMP_Text _sectionText;
        [SerializeField]
        private TMP_Text _lapText;
        [SerializeField]
        private TMP_Text _sectionTimeText;
        [SerializeField]
        private TMP_Text _splitTimeText;
        [SerializeField]
        private TMP_Text _lapTimeText;
        [SerializeField]
        private string _timeFormat = "hh\\:mm\\'ss\\\"fff";

        private CourseDescriptor _course;
        public CourseDescriptor Course
        {
            get => _course;
            set
            {
                _course = value;
                if (_courseNameText) { _courseNameText.text = _course ? _course.name : ""; }
            }
        }

        private int _numberOfLaps;
        public int NumberOfLaps
        {
            get => _numberOfLaps;
            set
            {
                _numberOfLaps = value;
                if (_numberOfLapsText) { _numberOfLapsText.text = _numberOfLaps.ToString(); }
            }
        }

        private VRCPlayerApi _driver;
        public VRCPlayerApi Driver
        {
            get => _driver;
            set
            {
                _driver = value;
                if (_driverNameText) { _driverNameText.text = Utilities.IsValid(value) ? value.displayName : ""; }
            }
        }

        private RaceRunner _runner;
        public RaceRunner Runner
        {
            get => _runner;
            set
            {
                _runner = value;
                if (_runnerVarietyText) { _runnerVarietyText.text = _runner ? _runner.variety : ""; }
            }
        }

        private int _section;
        public int Section
        {
            get => _section;
            set
            {
                _section = value;
                if (_sectionText) { _sectionText.text = _section.ToString(); }
            }
        }

        private int _lap;
        public int Lap
        {
            get => _lap;
            set
            {
                _lap = value;
                if (_lapText) { _lapText.text = _lap.ToString(); }
            }
        }

        private TimeSpan _sectionTime;
        public TimeSpan SectionTime
        {
            get => _sectionTime;
            set
            {
                _sectionTime = value;
                if (_sectionTimeText) { _sectionTimeText.text = _sectionTime.ToString(_timeFormat); }
            }
        }

        private TimeSpan _splitTime;
        public TimeSpan SplitTime
        {
            get => _splitTime;
            set
            {
                _splitTime = value;
                if (_splitTimeText) { _splitTimeText.text = _splitTime.ToString(_timeFormat); }
            }
        }

        private TimeSpan _lapTime;
        public TimeSpan LapTime
        {
            get => _lapTime;
            set
            {
                _lapTime = value;
                if (_lapTimeText) { _lapTimeText.text = _lapTime.ToString(_timeFormat); }
            }
        }
    }
}
