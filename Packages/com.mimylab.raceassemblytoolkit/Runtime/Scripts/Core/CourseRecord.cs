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
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Course Record")]
    [RequireComponent(typeof(VRCPlayerObject))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CourseRecord : UdonSharpBehaviour
    {
        [SerializeField]
        internal CourseDescriptor course;

        private RaceRunner[] _participateRunners = new RaceRunner[0];
        private RaceRunnerAsPlayer _participateRunnerAsPlayer;
        private RaceRunnerAsDrone _participateRunnerAsDrone;

        private int[] _numberOfLaps = new int[0];
        private int[] _latestSections = new int[0];
        private int[] _latestLaps = new int[0];
        private TimeSpan[] _latestSectionTimes = new TimeSpan[0];
        private TimeSpan[] _latestSplitTimes = new TimeSpan[0];
        private TimeSpan[] _latestLapTimes = new TimeSpan[0];
        private bool[] _areGoal = new bool[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _participateRunners = course.runners;
            _participateRunnerAsPlayer = course.runnerAsPlayer;
            _participateRunnerAsDrone = course.runnerAsDrone;

            _latestSections = new int[_participateRunners.Length];
            _latestLaps = new int[_participateRunners.Length];
            _latestSectionTimes = new TimeSpan[_participateRunners.Length];
            _latestSplitTimes = new TimeSpan[_participateRunners.Length];
            _latestLapTimes = new TimeSpan[_participateRunners.Length];
            _areGoal = new bool[_participateRunners.Length];

            _initialized = true;
        }
        private void Start()
        {
            Initialize();

            if (Networking.IsOwner(this.gameObject))
            {
                course.localCourseRecord = this;
            }
        }

        public override void OnPreSerialization()
        {

        }

        public override void OnDeserialization()
        {

        }

        internal void OnRunnerUpdate(RaceRunner runner)
        {
            var index = Array.IndexOf(_participateRunners, runner);
            if (index < 0) { return; }

            _latestSections[index] = runner.LatestSection;
            _latestLaps[index] = runner.LatestLap;
            _latestSectionTimes[index] = runner.LatestSectionTime;
            _latestSplitTimes[index] = runner.LatestSplitTime;
            _latestLapTimes[index] = runner.LatestLapTime;

        }

        internal void OnRunnerAsPlayerUpdate(RaceRunner runner) { }

        internal void OnRunnerAsDroneUpdate(RaceRunner runner) { }
    }
}
