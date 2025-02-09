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
    using VRC.Udon;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Runner")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Runner : UdonSharpBehaviour
    {
        [Header("Common Settings")]
        public string runnerName = "";

        [Header("Advanced Settings")]
        [SerializeField]
        private AudioSource _speaker;
        [SerializeField]
        private AudioClip _soundStart;
        [SerializeField]
        private AudioClip _soundCheckpoint;
        [SerializeField]
        private AudioClip _soundGoal;

        internal RunnerTimeDisplay timeDisplay;
        internal PlayerRecord playerRecord;

        private CourseDescriptor _entriedCourse;
        private Checkpoint[] _entriedCheckpoints = new Checkpoint[0];
        private Checkpoint _nextCheckpoint;
        private int _lapCount;
        private double[] _sectionClocks = new double[1];
        private int _currentSection;
        private int _currentLap;

        public CourseDescriptor EntriedCourse { get => _entriedCourse; }
        public int LapCount { get => _lapCount; }
        public int CurrentSection { get => _currentSection; }
        public int CurrentLap { get => _currentLap; }

        private void Update()
        {
            if (timeDisplay)
            {
                timeDisplay.CurrentTime = _isCounting ? GetCurrentTime() : GetGoalTime();
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

        private void EntryCourse(CourseDescriptor course)
        {
            _entriedCourse = course;
            _entriedCheckpoints = course.Checkpoints;
            _lapCount = course.LapCount;
            playerRecord = course.localPlayerRecord;

            var sectionCount = _lapCount > 0 ? _lapCount * _entriedCheckpoints.Length + 1 : _entriedCheckpoints.Length;
            _sectionClocks = new double[sectionCount];

            _nextCheckpoint = GetNextCheckpoint(_entriedCheckpoints[0]);

            if (timeDisplay)
            {
                timeDisplay.RunnerName = runnerName;
                timeDisplay.EntriedCourseName = _entriedCourse.courseName;
                timeDisplay.LapCount = _lapCount;
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

            if (timeDisplay)
            {
                timeDisplay.CurrentLap = _currentLap;
                timeDisplay.LastSectionTime = GetCurrentSectionTime();
                timeDisplay.LastSplitTime = GetCurrentSplitTime();
                timeDisplay.LastLapTime = GetCurrentLapTime();
            }
        }

        private void CountSection(double triggerClock)
        {
            _currentSection++;
            _sectionClocks[_currentSection] = triggerClock;

            if (timeDisplay)
            {
                timeDisplay.LastSectionTime = GetCurrentSectionTime();
                timeDisplay.LastSplitTime = GetCurrentSplitTime();
            }
        }

        private void CountLap()
        {
            _currentLap++;

            if (timeDisplay)
            {
                timeDisplay.CurrentLap = _currentLap;
                timeDisplay.LastLapTime = GetCurrentLapTime();
            }
        }

        private void CountStop(double triggerClock)
        {
            _nextCheckpoint = null;
            _isCounting = false;
        }
    }
}
