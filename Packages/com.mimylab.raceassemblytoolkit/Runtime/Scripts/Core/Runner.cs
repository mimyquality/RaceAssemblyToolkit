/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.Udon;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Runner")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Runner : UdonSharpBehaviour
    {
        private CourseDescriptor _entriedCourse = null;
        private Checkpoint[] _entriedCheckpoints = new Checkpoint[0];
        private Checkpoint _nextCheckpoint = null;
        private PlayerRecord _playerRecord = null;

        private int _lapCount;
        private double[] _sectionClocks = new double[0];

        private int _currentLap;
        private int _currentSection;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }



            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        private void OnTriggerEnter(Collider other)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(other)) { return; }

            var checkpoint = other.GetComponent<Checkpoint>();
            if (!checkpoint) { return; }

            if (checkpoint == _nextCheckpoint)
            {
                CountSection(triggerClock);

                if (checkpoint == _entriedCheckpoints[_entriedCheckpoints.Length - 1])
                {
                    if (_lapCount == 0)
                    {
                        CountStop(triggerClock);
                        ResetCourseEntry();
                        return;
                    }
                }

                if (checkpoint == _entriedCheckpoints[0])
                {
                    CountLap();
                    if (_currentLap >= _lapCount)
                    {
                        CountStop(triggerClock);
                        ResetCourseEntry();
                        return;
                    }
                }

                _nextCheckpoint = _entriedCourse.GetNextCheckpoint(checkpoint);
                return;
            }

            var course = checkpoint.course;
            if (!course) { return; }
            var checkpoints = course.Checkpoints;
            if (checkpoints.Length < 1) { return; }

            if (checkpoint == checkpoints[0])
            {
                EntryCourse(course);
                CountStart(triggerClock);
                return;
            }
        }

        private void ResetCourseEntry()
        {
            _entriedCourse = null;
            _playerRecord = null;
            _nextCheckpoint = null;
        }

        private void EntryCourse(CourseDescriptor course)
        {
            _entriedCourse = course;
            _entriedCheckpoints = course.Checkpoints;
            _lapCount = course.LapCount;
            _playerRecord = course.localPlayerRecord;

            var sectionCount = _lapCount > 0 ? _lapCount * _entriedCheckpoints.Length + 1 : _entriedCheckpoints.Length;
            _sectionClocks = new double[sectionCount];

            _nextCheckpoint = _entriedCourse.GetNextCheckpoint(_entriedCheckpoints[0]);
        }

        private void CountStart(double triggerClock)
        {
            _currentLap = 0;
            _currentSection = 0;
            _sectionClocks[_currentSection] = triggerClock;
        }

        private void CountSection(double triggerClock)
        {
            _currentSection++;
            _sectionClocks[_currentSection] = triggerClock;
        }

        private void CountLap()
        {
            _currentLap++;
        }

        private void CountStop(double triggerClock) { }
    }
}
