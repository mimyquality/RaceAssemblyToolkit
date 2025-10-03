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
        private const int RunnerAsLength = 2;

        [SerializeField]
        internal CourseDescriptor course;
        [SerializeField, HideInInspector]
        internal RaceRunner[] participateRunners = new RaceRunner[0];

        [UdonSynced] private byte[] sync_runnerStates = new byte[0];
        [UdonSynced] private int[] sync_numberOfLaps = new int[0];
        [UdonSynced] private int[] sync_latestSections = new int[0];
        [UdonSynced] private long[] sync_latestSectionTimes = new long[0];
        [UdonSynced] private long[] sync_latestLapTimes = new long[0];
        [UdonSynced] private long[] sync_latestSplitTimes = new long[0];

        [UdonSynced] private byte[] sync_runnerStatesAs = new byte[RunnerAsLength];
        [UdonSynced] private int[] sync_numberOfLapsAs = new int[RunnerAsLength];
        [UdonSynced] private int[] sync_latestSectionsAs = new int[RunnerAsLength];
        [UdonSynced] private long[] sync_latestSectionTimesAs = new long[RunnerAsLength];
        [UdonSynced] private long[] sync_latestLapTimesAs = new long[RunnerAsLength];
        [UdonSynced] private long[] sync_latestSplitTimesAs = new long[RunnerAsLength];

        private int _totalCheckpoints = 1;

        private bool[] _isEntries = new bool[0];
        private bool[] _isGoals = new bool[0];
        private int[] _numberOfLaps = new int[0];
        private int[] _latestSections = new int[0];
        private int[] _latestLaps = new int[0];
        private TimeSpan[] _latestSectionTimes = new TimeSpan[0];
        private TimeSpan[] _latestLapTimes = new TimeSpan[0];
        private TimeSpan[] _latestSplitTimes = new TimeSpan[0];

        private bool[] _isEntriesAs = new bool[RunnerAsLength];
        private bool[] _isGoalsAs = new bool[RunnerAsLength];
        private int[] _numberOfLapsAs = new int[RunnerAsLength];
        private int[] _latestSectionsAs = new int[RunnerAsLength];
        private int[] _latestLapsAs = new int[RunnerAsLength];
        private TimeSpan[] _latestSectionTimesAs = new TimeSpan[RunnerAsLength];
        private TimeSpan[] _latestLapTimesAs = new TimeSpan[RunnerAsLength];
        private TimeSpan[] _latestSplitTimesAs = new TimeSpan[RunnerAsLength];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _totalCheckpoints = course.checkpoints.Length;

            var count = participateRunners.Length;
            _isEntries = new bool[count];
            _isGoals = new bool[count];
            _numberOfLaps = new int[count];
            _latestSections = new int[count];
            _latestLaps = new int[count];
            _latestSectionTimes = new TimeSpan[count];
            _latestLapTimes = new TimeSpan[count];
            _latestSplitTimes = new TimeSpan[count];

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

        private bool _syncArrayInitialized = false;
        private void SyncArrayInitialize()
        {
            if (_syncArrayInitialized) { return; }

            var count = participateRunners.Length;
            sync_runnerStates = new byte[count];
            sync_numberOfLaps = new int[count];
            sync_latestSections = new int[count];
            sync_latestSectionTimes = new long[count];
            sync_latestLapTimes = new long[count];
            sync_latestSplitTimes = new long[count];

            _syncArrayInitialized = true;
        }
        public override void OnPreSerialization()
        {
            SyncArrayInitialize();

            for (int i = 0; i < participateRunners.Length; i++)
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

            for (int i = 0; i < RunnerAsLength; i++)
            {
                var runnerStateAs = 0;
                if (_isEntriesAs[i]) { runnerStateAs |= (int)CourseRecordRunnerStates.IsEntry; }
                if (_isGoalsAs[i]) { runnerStateAs |= (int)CourseRecordRunnerStates.IsGoal; }
                sync_runnerStatesAs[i] = (byte)runnerStateAs;
                sync_numberOfLapsAs[i] = _numberOfLapsAs[i];
                sync_latestSectionsAs[i] = _latestSectionsAs[i];
                sync_latestSectionTimesAs[i] = _latestSectionTimesAs[i].Ticks;
                sync_latestLapTimesAs[i] = _latestLapTimesAs[i].Ticks;
                sync_latestSplitTimesAs[i] = _latestSplitTimesAs[i].Ticks;
            }
        }

        public override void OnDeserialization()
        {
            Initialize();

            // RaceRunner の数が違う＝ 世界戦が変わっている(並び順も信用できないのでとりあえず破棄)
            if (participateRunners.Length != sync_numberOfLaps.Length) { return; }

            _numberOfLaps = sync_numberOfLaps;
            _latestSections = sync_latestSections;

            for (int i = 0; i < participateRunners.Length; i++)
            {
                _isEntries[i] = (sync_runnerStates[i] & (int)CourseRecordRunnerStates.IsEntry) > 0;
                _isGoals[i] = (sync_runnerStates[i] & (int)CourseRecordRunnerStates.IsGoal) > 0;

                _latestLaps[i] = _numberOfLaps[i] > 0 ? sync_latestSections[i] / _totalCheckpoints : sync_latestSections[i];

                _latestSectionTimes[i] = TimeSpan.FromTicks(sync_latestSectionTimes[i]);
                _latestLapTimes[i] = TimeSpan.FromTicks(sync_latestLapTimes[i]);
                _latestSplitTimes[i] = TimeSpan.FromTicks(sync_latestSplitTimes[i]);
            }

            _numberOfLapsAs = sync_numberOfLapsAs;
            _latestSectionsAs = sync_latestSectionsAs;

            for (int i = 0; i < RunnerAsLength; i++)
            {
                _isEntriesAs[i] = (sync_runnerStatesAs[i] & (int)CourseRecordRunnerStates.IsEntry) > 0;
                _isGoalsAs[i] = (sync_runnerStatesAs[i] & (int)CourseRecordRunnerStates.IsGoal) > 0;

                _latestLapsAs[i] = _numberOfLapsAs[i] > 0 ? sync_latestSectionsAs[i] / _totalCheckpoints : sync_latestSectionsAs[i];

                _latestSectionTimesAs[i] = TimeSpan.FromTicks(sync_latestSectionTimesAs[i]);
                _latestLapTimesAs[i] = TimeSpan.FromTicks(sync_latestLapTimesAs[i]);
                _latestSplitTimesAs[i] = TimeSpan.FromTicks(sync_latestSplitTimesAs[i]);
            }

            // ToDo:ランキングボードに更新通知処理
        }

        internal void OnRunnerUpdate(RaceRunner runner)
        {
            Initialize();

            var index = Array.IndexOf(participateRunners, runner);
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

            // ToDo:ランキングボードに更新通知処理
        }

        internal void OnRunnerAsPlayerUpdate(RaceRunner runner)
        {
            Initialize();

            _isEntriesAs[0] = runner.IsEntry;
            _isGoalsAs[0] = runner.IsGoal;
            _numberOfLapsAs[0] = runner.NumberOfLaps;
            _latestSectionsAs[0] = runner.LatestSection;
            _latestLapsAs[0] = runner.LatestLap;
            _latestSectionTimesAs[0] = runner.LatestSectionTime;
            _latestLapTimesAs[0] = runner.LatestLapTime;
            _latestSplitTimesAs[0] = runner.LatestSplitTime;

            RequestSerialization();

            // ToDo:ランキングボードに更新通知処理
        }

        internal void OnRunnerAsDroneUpdate(RaceRunner runner)
        {
            Initialize();

            _isEntriesAs[1] = runner.IsEntry;
            _isGoalsAs[1] = runner.IsGoal;
            _numberOfLapsAs[1] = runner.NumberOfLaps;
            _latestSectionsAs[1] = runner.LatestSection;
            _latestLapsAs[1] = runner.LatestLap;
            _latestSectionTimesAs[1] = runner.LatestSectionTime;
            _latestLapTimesAs[1] = runner.LatestLapTime;
            _latestSplitTimesAs[1] = runner.LatestSplitTime;

            RequestSerialization();

            // ToDo:ランキングボードに更新通知処理
        }
    }
}
