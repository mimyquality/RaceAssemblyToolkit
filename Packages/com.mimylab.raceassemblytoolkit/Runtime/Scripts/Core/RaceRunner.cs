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
    using VRC.SDKBase;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Runner")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RaceRunner : UdonSharpBehaviour
    {
        [Header("Base Settings")]
        public string variety = "";

        [Header("Additional Settings")]
        [SerializeField]
        private AudioSource _speaker;
        [SerializeField]
        private AudioClip _soundStart;
        [SerializeField]
        private AudioClip _soundCheckpoint;
        [SerializeField]
        private AudioClip _soundGoal;

        internal PlayerRecord playerRecord;

        private RaceRunnerTimeDisplay _timeDisplay;
        private VRCPlayerApi _driver;
        private CourseDescriptor _entriedCourse;
        private Checkpoint[] _entriedCheckpoints = new Checkpoint[0];
        private Checkpoint _nextCheckpoint;
        private int _lapCount;
        private double[] _sectionClocks = new double[1];
        private TimeSpan _currentTime;
        private int _currentSection;
        private int _currentLap;

        private void Update()
        {
            if (_timeDisplay)
            {
                var currentTime = _isCounting ? GetCurrentTime() : GetGoalTime();
                if (currentTime != _currentTime)
                {
                    _currentTime = currentTime;
                    _timeDisplay.CurrentTime = currentTime;
                }
            }
        }

        public void OnCheckpointPassed(Checkpoint checkpoint, double checkClock)
        {
            if (checkpoint == _nextCheckpoint)
            {
                CountSection(checkClock);

                if (checkpoint == _entriedCheckpoints[_entriedCheckpoints.Length - 1])
                {
                    if (_lapCount == 0)
                    {
                        CountStop(checkClock);
                        return;
                    }
                }

                if (checkpoint == _entriedCheckpoints[0])
                {
                    CountLap();
                    if (_currentLap >= _lapCount)
                    {
                        CountStop(checkClock);
                        return;
                    }
                }

                _nextCheckpoint = GetNextCheckpoint(checkpoint);
                return;
            }

            var course = checkpoint.course;
            if (!course) { return; }
            var checkpoints = course.Checkpoints;
            if (checkpoints.Length < 1) { return; }

            if (checkpoint == checkpoints[0])
            {
                EntryCourse(course);
                CountStart(checkClock);
                return;
            }
        }

        public VRCPlayerApi GetDriver()
        {
            return _driver;
        }

        public TimeSpan GetSectionTime(int section)
        {
            if (section < 1) { return TimeSpan.Zero; }
            if (section >= _sectionClocks.Length) { return TimeSpan.Zero; }
            if (_sectionClocks[section] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_sectionClocks[section] - _sectionClocks[section - 1]);
        }

        public TimeSpan GetSplitTime(int section)
        {
            if (section < 1) { return TimeSpan.Zero; }
            if (section >= _sectionClocks.Length) { return TimeSpan.Zero; }
            if (_sectionClocks[section] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_sectionClocks[section] - _sectionClocks[0]);
        }

        public TimeSpan GetLapTime(int lap)
        {
            // ワンパスモードならセクション時間を返す
            if (_lapCount < 1) { return GetSectionTime(lap); }

            if (lap < 1) { return TimeSpan.Zero; }
            if (lap > _lapCount) { return TimeSpan.Zero; }

            lap = lap * _entriedCheckpoints.Length;
            if (_sectionClocks[lap] == 0.0d) { return TimeSpan.Zero; }
            return TimeSpan.FromSeconds(_sectionClocks[lap] - _sectionClocks[lap - _entriedCheckpoints.Length]);
        }

        public TimeSpan GetCurrentTime()
        {
            if (_sectionClocks[0] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(Time.timeAsDouble - _sectionClocks[0]);
        }

        public TimeSpan GetCurrentSectionTime()
        {
            return GetSectionTime(_currentSection);
        }

        public TimeSpan GetCurrentSplitTime()
        {
            return GetSplitTime(_currentSection);
        }

        public TimeSpan GetCurrentLapTime()
        {
            return GetLapTime(_currentLap);
        }

        public TimeSpan GetGoalTime()
        {
            return GetSplitTime(_sectionClocks.Length - 1);
        }

        internal void SetDriver(VRCPlayerApi driver)
        {
            if (!Utilities.IsValid(driver)) { return; }

            if (this._driver != driver)
            {
                this._driver = driver;
                CountReset();

                if (_timeDisplay)
                {
                    _timeDisplay.DriverName = driver.displayName;
                }
            }
        }

        internal void SetTimeDisplay(RaceRunnerTimeDisplay timeDisplay)
        {
            if (!timeDisplay) { return; }

            _timeDisplay = timeDisplay;
            _timeDisplay.RunnerVariety = variety;
        }

        private void EntryCourse(CourseDescriptor course)
        {
            _entriedCourse = course;
            _entriedCheckpoints = course.Checkpoints;
            _lapCount = course.LapCount;
            playerRecord = course.localPlayerRecord;

            var sectionCount = _lapCount > 0 ? _lapCount * _entriedCheckpoints.Length + 1 : _entriedCheckpoints.Length;
            _sectionClocks = new double[sectionCount];

            _nextCheckpoint = GetNextCheckpoint(_entriedCheckpoints[0]);

            if (_timeDisplay)
            {
                _timeDisplay.EntriedCourseName = _entriedCourse.courseName;
                _timeDisplay.LapCount = _lapCount;
            }
        }

        private Checkpoint GetNextCheckpoint(Checkpoint currentCheckpoint)
        {
            var current = Array.IndexOf(_entriedCheckpoints, currentCheckpoint);
            if (current < 0) { return null; }

            if (current >= _entriedCheckpoints.Length - 1)
            {
                return _entriedCheckpoints[0];
            }

            return _entriedCheckpoints[current + 1];
        }

        private bool _isCounting;

        private void CountReset()
        {
            _entriedCourse = null;
            _entriedCheckpoints = new Checkpoint[0];
            _lapCount = 0;
            playerRecord = null;
            _nextCheckpoint = null;
            _currentLap = 0;
            _currentSection = 0;
            _sectionClocks = new double[1];
            _isCounting = false;

            if (_timeDisplay)
            {
                _timeDisplay.EntriedCourseName = "";
                _timeDisplay.LapCount = _lapCount;
                _timeDisplay.CurrentLap = _currentLap;
                _timeDisplay.LastSectionTime = GetCurrentSectionTime();
                _timeDisplay.LastSplitTime = GetCurrentSplitTime();
                _timeDisplay.LastLapTime = GetCurrentLapTime();
            }
        }

        private void CountStart(double triggerClock)
        {
            _currentLap = 0;
            _currentSection = 0;
            _sectionClocks[_currentSection] = triggerClock;
            _isCounting = true;

            if (_timeDisplay)
            {
                _timeDisplay.CurrentLap = _currentLap;
                _timeDisplay.LastSectionTime = GetCurrentSectionTime();
                _timeDisplay.LastSplitTime = GetCurrentSplitTime();
                _timeDisplay.LastLapTime = GetCurrentLapTime();
            }

            if (_speaker && _soundStart) { _speaker.PlayOneShot(_soundStart); }
        }

        private void CountSection(double triggerClock)
        {
            _currentSection++;
            _sectionClocks[_currentSection] = triggerClock;

            if (_timeDisplay)
            {
                _timeDisplay.LastSectionTime = GetCurrentSectionTime();
                _timeDisplay.LastSplitTime = GetCurrentSplitTime();
            }

            if (_speaker && _soundCheckpoint) { _speaker.PlayOneShot(_soundCheckpoint); }
        }

        private void CountLap()
        {
            _currentLap++;

            if (_timeDisplay)
            {
                _timeDisplay.CurrentLap = _currentLap;
                _timeDisplay.LastLapTime = GetCurrentLapTime();
            }
        }

        private void CountStop(double triggerClock)
        {
            _nextCheckpoint = null;
            _isCounting = false;

            if (_speaker && _soundGoal) { _speaker.PlayOneShot(_soundGoal); }
        }
    }
}
