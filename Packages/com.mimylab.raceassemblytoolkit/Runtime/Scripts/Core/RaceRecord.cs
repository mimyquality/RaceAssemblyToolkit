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


    [Flags]
    public enum CourseRecordRunnerStates
    {
        None = 0,
        IsEntry = 1 << 0,
        IsGoal = 1 << 1
    }

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Record")]
    [RequireComponent(typeof(VRCPlayerObject))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RaceRecord : UdonSharpBehaviour
    {
        [SerializeField]
        internal CourseDescriptor course;

        [UdonSynced] private byte[] sync_runnerStates = new byte[0];
        [UdonSynced] private int[] sync_numberOfLaps = new int[0];
        [UdonSynced] private int[] sync_latestSections = new int[0];
        [UdonSynced] private long[] sync_latestSectionTimes = new long[0];
        [UdonSynced] private long[] sync_latestLapTimes = new long[0];
        [UdonSynced] private long[] sync_latestSplitTimes = new long[0];

        private RaceRunner[] _participateRunners = new RaceRunner[0];
        private RaceRunner[] _localParticipateRunners;
        private int _totalCheckpoints = 1;
        private int _runnersLength = 0;

        private bool[] _isEntries = new bool[0];
        private bool[] _isGoals = new bool[0];
        private int[] _numberOfLaps = new int[0];
        private int[] _latestSections = new int[0];
        private int[] _latestLaps = new int[0];
        private TimeSpan[] _latestSectionTimes = new TimeSpan[0];
        private TimeSpan[] _latestLapTimes = new TimeSpan[0];
        private TimeSpan[] _latestSplitTimes = new TimeSpan[0];

        private IRecordReceiver[] _receivers = new IRecordReceiver[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _totalCheckpoints = course.checkpoints.Length;
            _participateRunners = course.raceRunners;
            _runnersLength = _participateRunners.Length;

            _isEntries = new bool[_runnersLength];
            _isGoals = new bool[_runnersLength];
            _numberOfLaps = new int[_runnersLength];
            _latestSections = new int[_runnersLength];
            _latestLaps = new int[_runnersLength];
            _latestSectionTimes = new TimeSpan[_runnersLength];
            _latestLapTimes = new TimeSpan[_runnersLength];
            _latestSplitTimes = new TimeSpan[_runnersLength];

            _initialized = true;
        }
        private void Start()
        {
            Initialize();

            if (Networking.GetOwner(this.gameObject).isLocal)
            {
                course.localRaceRecord = this;
            }
        }

        public override void OnPreSerialization()
        {
            if (sync_runnerStates.Length != _runnersLength) { sync_runnerStates = new byte[_runnersLength]; }
            if (sync_numberOfLaps.Length != _runnersLength) { sync_numberOfLaps = new int[_runnersLength]; }
            if (sync_latestSections.Length != _runnersLength) { sync_latestSections = new int[_runnersLength]; }
            if (sync_latestSectionTimes.Length != _runnersLength) { sync_latestSectionTimes = new long[_runnersLength]; }
            if (sync_latestLapTimes.Length != _runnersLength) { sync_latestLapTimes = new long[_runnersLength]; }
            if (sync_latestSplitTimes.Length != _runnersLength) { sync_latestSplitTimes = new long[_runnersLength]; }

            for (int i = 0; i < _runnersLength; i++)
            {
                var runnerState = 0;
                if (_isEntries[i]) { runnerState |= (int)CourseRecordRunnerStates.IsEntry; }
                if (_isGoals[i]) { runnerState |= (int)CourseRecordRunnerStates.IsGoal; }
                sync_runnerStates[i] = (byte)runnerState;
                sync_numberOfLaps[i] = _numberOfLaps[i];
                sync_latestSections[i] = _latestSections[i];
                sync_latestSectionTimes[i] = _latestSectionTimes[i].Ticks;
                sync_latestLapTimes[i] = _latestLapTimes[i].Ticks;
                sync_latestSplitTimes[i] = _latestSplitTimes[i].Ticks;
            }
        }

        public override void OnDeserialization()
        {
            Initialize();

            // RaceRunner の数が違う＝ 世界戦が変わっている(並び順も信用できないのでとりあえず破棄)
            if (_runnersLength != sync_numberOfLaps.Length) { return; }

            for (int i = 0; i < _runnersLength; i++)
            {
                _isEntries[i] = (sync_runnerStates[i] & (int)CourseRecordRunnerStates.IsEntry) > 0;
                _isGoals[i] = (sync_runnerStates[i] & (int)CourseRecordRunnerStates.IsGoal) > 0;
                _numberOfLaps[i] = sync_numberOfLaps[i];
                _latestSections[i] = sync_latestSections[i];
                _latestLaps[i] = sync_numberOfLaps[i] > 0 ? sync_latestSections[i] / _totalCheckpoints : sync_latestSections[i];
                _latestSectionTimes[i] = TimeSpan.FromTicks(sync_latestSectionTimes[i]);
                _latestLapTimes[i] = TimeSpan.FromTicks(sync_latestLapTimes[i]);
                _latestSplitTimes[i] = TimeSpan.FromTicks(sync_latestSplitTimes[i]);
            }

            SendRaceRecordUpdate();
        }

        internal void OnRunnerUpdate(RaceRunner runner)
        {
            Initialize();

            if (_localParticipateRunners == null)
            {
                var localPlayer = Networking.LocalPlayer;
                _localParticipateRunners = new RaceRunner[_runnersLength];
                for (int i = 0; i < _localParticipateRunners.Length; i++)
                {
                    _localParticipateRunners[i] = (RaceRunner)localPlayer.FindComponentInPlayerObjects(_participateRunners[i]);
                }
            }
            var index = Array.IndexOf(_localParticipateRunners, runner);
            if (index < 0) { return; }

            _isEntries[index] = runner.IsEntry;
            _isGoals[index] = runner.IsGoal;
            _numberOfLaps[index] = runner.NumberOfLaps;
            _latestSections[index] = runner.LatestSection;
            _latestLaps[index] = runner.LatestLap;
            _latestSectionTimes[index] = runner.LatestSectionTime;
            _latestLapTimes[index] = runner.LatestLapTime;
            _latestSplitTimes[index] = runner.LatestSplitTime;

            RequestSerialization();

            SendRaceRecordUpdate();
        }

        internal void AddRecordReveiver(IRecordReceiver receiver)
        {
            if (Array.IndexOf(_receivers, receiver) > -1) { return; }

            var tmp_receivers = new IRecordReceiver[_receivers.Length + 1];
            _receivers.CopyTo(tmp_receivers, 0);
            tmp_receivers[_receivers.Length] = receiver;
            _receivers = tmp_receivers;
        }

        internal void RemoveRecordReveiver(IRecordReceiver receiver)
        {
            var index = Array.IndexOf(_receivers, receiver);
            if (index < 0) { return; }

            var tmp_receivers = new IRecordReceiver[_receivers.Length - 1];
            Array.Copy(_receivers, 0, tmp_receivers, 0, index);
            Array.Copy(_receivers, index + 1, tmp_receivers, index, tmp_receivers.Length - index);
            _receivers = tmp_receivers;
        }

        private void SendRaceRecordUpdate()
        {
            for (int i = 0; i < _receivers.Length; i++)
            {
                if (_receivers[i])
                {
                    _receivers[i].OnRaceRecordUpdate(this);
                }
            }
        }
    }
}
