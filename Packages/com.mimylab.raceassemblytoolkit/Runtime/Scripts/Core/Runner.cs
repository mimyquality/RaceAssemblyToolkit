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
    using VRC.Udon;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Runner")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Runner : UdonSharpBehaviour
    {
        public string runnerName = "";

        internal RunnerTimeDisplay display;

        private CourseDescriptor _entriedCourse = null;
        private Checkpoint[] _entriedCheckpoints = new Checkpoint[0];
        private Checkpoint _nextCheckpoint = null;
        private PlayerRecord _playerRecord = null;

        private int _lapCount;
        public double[] _sectionClocks = new double[1];
        public int _currentSection;
        public int _currentLap;

        public CourseDescriptor EntriedCourse { get => _entriedCourse; }
        public int LapCount { get => _lapCount; }
        public int CurrentSection { get => _currentSection; }
        public int CurrentLap { get => _currentLap; }

        private void Update()
        {
            if (display)
            {
                display.CurrentTime = _isCounting ? GetCurrentTime() : GetGoalTime();
            }
        }

        public void OnPassCheckpoint(Checkpoint checkpoint, double checkClock)
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

        public TimeSpan GetSectionTime(int section)
        {
            if (section < 1) { return TimeSpan.Zero; }
            if (section >= _sectionClocks.Length) { return TimeSpan.Zero; }
            if (_sectionClocks[section] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_sectionClocks[section] - _sectionClocks[section - 1]);
        }

        public TimeSpan GetLapTime(int lap)
        {
            // ToDo:セクション間隔のままなのでラップ間隔に要修正

            if (lap < 1) { return TimeSpan.Zero; }
            if (lap > _lapCount) { return TimeSpan.Zero; }
            if (_sectionClocks[lap] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_sectionClocks[lap] - _sectionClocks[lap - 1]);
        }

        public TimeSpan GetSplitTime(int section)
        {
            if (section < 1) { return TimeSpan.Zero; }
            if (section >= _sectionClocks.Length) { return TimeSpan.Zero; }
            if (_sectionClocks[section] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_sectionClocks[section] - _sectionClocks[0]);
        }

        public TimeSpan GetCurrentTime()
        {
            if (_sectionClocks[0] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(Time.timeAsDouble - _sectionClocks[0]);
        }

        public TimeSpan GetCurrentLapTime()
        {
            return GetLapTime(_currentSection);
        }

        public TimeSpan GetCurrentSplitTime()
        {
            return GetSplitTime(_currentSection);
        }

        public TimeSpan GetGoalTime()
        {
            return GetSplitTime(_sectionClocks.Length - 1);
        }

        private void EntryCourse(CourseDescriptor course)
        {
            _entriedCourse = course;
            _entriedCheckpoints = course.Checkpoints;
            _lapCount = course.LapCount;
            _playerRecord = course.localPlayerRecord;

            var sectionCount = _lapCount > 0 ? _lapCount * _entriedCheckpoints.Length + 1 : _entriedCheckpoints.Length;
            _sectionClocks = new double[sectionCount];

            _nextCheckpoint = GetNextCheckpoint(_entriedCheckpoints[0]);

            if (display)
            {
                display.RunnerName = runnerName;
                display.EntriedCourseName = _entriedCourse.courseName;
                display.LapCount = _lapCount;
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

        private void CountStart(double triggerClock)
        {
            _currentLap = 0;
            _currentSection = 0;
            _sectionClocks[_currentSection] = triggerClock;
            _isCounting = true;

            if (display)
            {
                display.CurrentLap = _currentLap;
            }
        }

        private void CountSection(double triggerClock)
        {
            _currentSection++;
            _sectionClocks[_currentSection] = triggerClock;

            display.LastLapTime = GetLapTime(_currentSection);
            display.LastSplitTime = GetSplitTime(_currentSection);
        }

        private void CountLap()
        {
            _currentLap++;

            if (display)
            {
                display.CurrentLap = _currentLap;
            }
        }

        private void CountStop(double triggerClock)
        {
            _nextCheckpoint = null;
            _isCounting = false;
        }
    }
}
