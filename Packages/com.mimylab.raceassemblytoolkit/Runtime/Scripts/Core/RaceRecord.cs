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

        [UdonSynced] private byte sync_runnerStateAsPlayer;
        [UdonSynced] private int sync_numberOfLapsAsPlayer;
        [UdonSynced] private int sync_latestSectionAsPlayer;
        [UdonSynced] private long sync_latestSectionTimeAsPlayer;
        [UdonSynced] private long sync_latestLapTimeAsPlayer;
        [UdonSynced] private long sync_latestSplitTimeAsPlayer;

        [UdonSynced] private byte sync_runnerStateAsDrone;
        [UdonSynced] private int sync_numberOfLapsAsDrone;
        [UdonSynced] private int sync_latestSectionAsDrone;
        [UdonSynced] private long sync_latestSectionTimeAsDrone;
        [UdonSynced] private long sync_latestLapTimeAsDrone;
        [UdonSynced] private long sync_latestSplitTimeAsDrone;

        private RaceRunner[] _participateRunners = new RaceRunner[0];
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

        private bool _isEntryAsPlayer;
        private bool _isGoalAsPlayer;
        private int _numberOfLapsAsPlayer;
        private int _latestSectionAsPlayer;
        private int _latestLapAsPlayer;
        private TimeSpan _latestSectionTimeAsPlayer;
        private TimeSpan _latestLapTimeAsPlayer;
        private TimeSpan _latestSplitTimeAsPlayer;

        private bool _isEntryAsDrone;
        private bool _isGoalAsDrone;
        private int _numberOfLapsAsDrone;
        private int _latestSectionAsDrone;
        private int _latestLapAsDrone;
        private TimeSpan _latestSectionTimeAsDrone;
        private TimeSpan _latestLapTimeAsDrone;
        private TimeSpan _latestSplitTimeAsDrone;

        private IRecordReceiver[] _receivers = new IRecordReceiver[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _participateRunners = course.Runners;
            _totalCheckpoints = course.checkpoints.Length;
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

            if (Networking.IsOwner(this.gameObject))
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

            int runnerState;
            for (int i = 0; i < _runnersLength; i++)
            {
                runnerState = 0;
                if (_isEntries[i]) { runnerState |= (int)CourseRecordRunnerStates.IsEntry; }
                if (_isGoals[i]) { runnerState |= (int)CourseRecordRunnerStates.IsGoal; }
                sync_runnerStates[i] = (byte)runnerState;
                sync_numberOfLaps[i] = _numberOfLaps[i];
                sync_latestSections[i] = _latestSections[i];
                sync_latestSectionTimes[i] = _latestSectionTimes[i].Ticks;
                sync_latestLapTimes[i] = _latestLapTimes[i].Ticks;
                sync_latestSplitTimes[i] = _latestSplitTimes[i].Ticks;
            }

            runnerState = 0;
            if (_isEntryAsPlayer) { runnerState |= (int)CourseRecordRunnerStates.IsEntry; }
            if (_isGoalAsPlayer) { runnerState |= (int)CourseRecordRunnerStates.IsGoal; }
            sync_runnerStateAsPlayer = (byte)runnerState;
            sync_numberOfLapsAsPlayer = _numberOfLapsAsPlayer;
            sync_latestSectionAsPlayer = _latestSectionAsPlayer;
            sync_latestSectionTimeAsPlayer = _latestSectionTimeAsPlayer.Ticks;
            sync_latestLapTimeAsPlayer = _latestLapTimeAsPlayer.Ticks;
            sync_latestSplitTimeAsPlayer = _latestSplitTimeAsPlayer.Ticks;

            runnerState = 0;
            if (_isEntryAsDrone) { runnerState |= (int)CourseRecordRunnerStates.IsEntry; }
            if (_isGoalAsDrone) { runnerState |= (int)CourseRecordRunnerStates.IsGoal; }
            sync_runnerStateAsDrone = (byte)runnerState;
            sync_numberOfLapsAsDrone = _numberOfLapsAsDrone;
            sync_latestSectionAsDrone = _latestSectionAsDrone;
            sync_latestSectionTimeAsDrone = _latestSectionTimeAsDrone.Ticks;
            sync_latestLapTimeAsDrone = _latestLapTimeAsDrone.Ticks;
            sync_latestSplitTimeAsDrone = _latestSplitTimeAsDrone.Ticks;
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

            _isEntryAsPlayer = (sync_runnerStateAsPlayer & (int)CourseRecordRunnerStates.IsEntry) > 0;
            _isGoalAsPlayer = (sync_runnerStateAsPlayer & (int)CourseRecordRunnerStates.IsGoal) > 0;
            _numberOfLapsAsPlayer = sync_numberOfLapsAsPlayer;
            _latestSectionAsPlayer = sync_latestSectionAsPlayer;
            _latestLapAsPlayer = sync_numberOfLapsAsPlayer > 0 ? sync_latestSectionAsPlayer / _totalCheckpoints : sync_latestSectionAsPlayer;
            _latestSectionTimeAsPlayer = TimeSpan.FromTicks(sync_latestSectionTimeAsPlayer);
            _latestLapTimeAsPlayer = TimeSpan.FromTicks(sync_latestLapTimeAsPlayer);
            _latestSplitTimeAsPlayer = TimeSpan.FromTicks(sync_latestSplitTimeAsPlayer);

            _isEntryAsDrone = (sync_runnerStateAsDrone & (int)CourseRecordRunnerStates.IsEntry) > 0;
            _isGoalAsDrone = (sync_runnerStateAsDrone & (int)CourseRecordRunnerStates.IsGoal) > 0;
            _numberOfLapsAsDrone = sync_numberOfLapsAsDrone;
            _latestSectionAsDrone = sync_latestSectionAsDrone;
            _latestLapAsDrone = sync_numberOfLapsAsDrone > 0 ? sync_latestSectionAsDrone / _totalCheckpoints : sync_latestSectionAsDrone;
            _latestSectionTimeAsDrone = TimeSpan.FromTicks(sync_latestSectionTimeAsDrone);
            _latestLapTimeAsDrone = TimeSpan.FromTicks(sync_latestLapTimeAsDrone);
            _latestSplitTimeAsDrone = TimeSpan.FromTicks(sync_latestSplitTimeAsDrone);

            UpdateBoard();
        }

        internal void OnRunnerUpdate(RaceRunner runner)
        {
            Initialize();

            var index = Array.IndexOf(_participateRunners, runner);
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

            UpdateBoard();
        }

        internal void OnRunnerAsPlayerUpdate(RaceRunner runner)
        {
            Initialize();

            _isEntryAsPlayer = runner.IsEntry;
            _isGoalAsPlayer = runner.IsGoal;
            _numberOfLapsAsPlayer = runner.NumberOfLaps;
            _latestSectionAsPlayer = runner.LatestSection;
            _latestLapAsPlayer = runner.LatestLap;
            _latestSectionTimeAsPlayer = runner.LatestSectionTime;
            _latestLapTimeAsPlayer = runner.LatestLapTime;
            _latestSplitTimeAsPlayer = runner.LatestSplitTime;

            RequestSerialization();

            UpdateBoard();
        }

        internal void OnRunnerAsDroneUpdate(RaceRunner runner)
        {
            Initialize();

            _isEntryAsDrone = runner.IsEntry;
            _isGoalAsDrone = runner.IsGoal;
            _numberOfLapsAsDrone = runner.NumberOfLaps;
            _latestSectionAsDrone = runner.LatestSection;
            _latestLapAsDrone = runner.LatestLap;
            _latestSectionTimeAsDrone = runner.LatestSectionTime;
            _latestLapTimeAsDrone = runner.LatestLapTime;
            _latestSplitTimeAsDrone = runner.LatestSplitTime;

            RequestSerialization();

            UpdateBoard();
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

        private void UpdateBoard()
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
