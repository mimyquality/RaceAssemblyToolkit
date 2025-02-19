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
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Player Record")]
    [RequireComponent(typeof(VRCPlayerObject), typeof(VRCEnablePersistence))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerRecord : UdonSharpBehaviour
    {
        internal CourseDescriptor course;

        private TimeSpan _bestRecordTime;
        private string _bestRecordRunnerName = "";

        private TimeSpan _recordTime;
        private RaceRunner _recordRunner;
        private string _recordRunnerName = "";

        private TimeSpan[] _lapTimes = new TimeSpan[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (Networking.IsOwner(this.gameObject))
            {
                course.localPlayerRecord = this;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();


        }

        public void AddSplitTime(TimeSpan splitTime)
        {

        }

        public void AddLapTime(TimeSpan lapTime)
        {

        }

        public void SetGoalTime(TimeSpan goalTime)
        {

        }
    }
}
