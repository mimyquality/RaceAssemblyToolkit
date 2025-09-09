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
    using VRC.SDK3.Components;
    using VRC.SDKBase;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Course Record")]
    [RequireComponent(typeof(VRCPlayerObject))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CourseRecord : UdonSharpBehaviour
    {
        [SerializeField]
        private CourseDescriptor _course;

        [UdonSynced] private int sync_numberOfLaps;
        [UdonSynced] private int sync_latestSection;
        [UdonSynced] private int sync_latestLap;
        [UdonSynced] private long sync_latestSectionTime;
        [UdonSynced] private long sync_latestSplitTime;
        [UdonSynced] private long sync_latestLapTime;
        [UdonSynced] private bool sync_isGoal;

        internal RaceRunner[] participateRunners = new RaceRunner[0];
        internal RaceRunnerAsPlayer participateRunnerAsPlayer;
        internal RaceRunnerAsDrone participateRunnerAsDrone;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (Networking.IsOwner(this.gameObject))
            {
                _course.localCourseRecord = this;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void OnPreSerialization()
        {
        }

        public override void OnDeserialization()
        {
            Initialize();

            var _entriedNumberOfLaps = sync_numberOfLaps;
            var _latestSection = sync_latestSection;
            var _latestLap = sync_latestLap;
            var _latestSectionTime = TimeSpan.FromTicks(sync_latestSectionTime);
            var _latestSplitTime = TimeSpan.FromTicks(sync_latestSplitTime);
            var _latestLapTime = TimeSpan.FromTicks(sync_latestLapTime);
            var _isGoal = sync_isGoal;
        }
    }
}
