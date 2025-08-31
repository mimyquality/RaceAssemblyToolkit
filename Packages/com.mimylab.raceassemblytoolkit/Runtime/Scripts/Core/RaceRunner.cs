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

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Runner")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RaceRunner : UdonSharpBehaviour
    {
        [Header("Require References")]
        [SerializeField]
        private Stopwatch _stopwatch;

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

        internal PersonalRecord personalRecord;

        [UdonSynced]
        private int sync_numberOfLaps;
        [UdonSynced]
        private int sync_latestSection;
        [UdonSynced]
        private int sync_latestLap;
        [UdonSynced]
        private long sync_latestSectionTime;
        [UdonSynced]
        private long sync_latestSplitTime;
        [UdonSynced]
        private long sync_latestLapTime;
        [UdonSynced]
        private bool sync_isGoal;

        private VRCPlayerApi _driver;
        private CourseDescriptor _entriedCourse;
        private Checkpoint[] _entriedCheckpoints = new Checkpoint[0];
        private int _entriedNumberOfLaps;
        private Checkpoint _nextCheckpoint;
        private int _latestSection;
        private int _latestLap;
        private TimeSpan _latestSectionTime;
        private TimeSpan _latestSplitTime;
        private TimeSpan _latestLapTime;
        private bool _isGoal;

        public CourseDescriptor EntriedCourse { get => _entriedCourse; }
        public int NumberOfLaps { get => _entriedNumberOfLaps; }
        public int LatestSection { get => _latestSection; }
        public int LatestLap { get => _entriedNumberOfLaps > 0 ? _latestLap : _latestSection; }
        public TimeSpan LatestSectionTime { get => _latestSectionTime; }
        public TimeSpan LatestSplitTime { get => _latestSplitTime; }
        public TimeSpan LatestLapTime { get => _latestLapTime; }
        public TimeSpan GoalTime { get => _isGoal ? _latestSplitTime : TimeSpan.Zero; }

        public override void OnPreSerialization()
        {
            sync_numberOfLaps = _entriedNumberOfLaps;
            sync_latestSection = _latestSection;
            sync_latestLap = _latestLap;
            sync_latestSectionTime = _latestSectionTime.Ticks;
            sync_latestSplitTime = _latestSplitTime.Ticks;
            sync_latestLapTime = _latestLapTime.Ticks;
            sync_isGoal = _isGoal;
        }

        public override void OnDeserialization()
        {
            _entriedNumberOfLaps = sync_numberOfLaps;
            _latestSection = sync_latestSection;
            _latestLap = sync_latestLap;
            _latestSectionTime = TimeSpan.FromTicks(sync_latestSectionTime);
            _latestSplitTime = TimeSpan.FromTicks(sync_latestSplitTime);
            _latestLapTime = TimeSpan.FromTicks(sync_latestLapTime);
            _isGoal = sync_isGoal;
        }

        public VRCPlayerApi GetDriver()
        {
            return _driver;
        }

        public TimeSpan _GetSectionTime(int section)
        {
            return _stopwatch.GetLapTime(section);
        }

        public TimeSpan _GetSplitTime(int section)
        {
            return _stopwatch.GetSplitTime(section);
        }

        public TimeSpan _GetLapTime(int lap)
        {
            if (lap < 1) { return TimeSpan.Zero; }

            // ワンパスモードならセクション時間を返す
            if (_entriedNumberOfLaps < 1) { return _GetLapTime(lap); }

            var section = lap * _entriedCheckpoints.Length;
            return _stopwatch.GetLapTime(section, section - _entriedCheckpoints.Length);
        }

        public TimeSpan _GetSplitTimeByLap(int lap)
        {
            if (lap < 1) { return TimeSpan.Zero; }

            // ワンパスモードならセクション時間を返す
            if (_entriedNumberOfLaps < 1) { return _GetSectionTime(lap); }

            var section = lap * _entriedCheckpoints.Length;
            return _stopwatch.GetSplitTime(section);
        }

        public TimeSpan _GetTotalTime()
        {
            return _stopwatch.GetTotalTime();
        }

        public TimeSpan _GetCurrentTime()
        {
            return _stopwatch.GetCurrentTime();
        }

        internal void OnCheckpointPassed(Checkpoint checkpoint, double triggerClock)
        {
            // 次のチェックポイント通過処理
            if (checkpoint == _nextCheckpoint)
            {
                CountSection(triggerClock);

                // ワンパスモード
                if (_entriedNumberOfLaps < 1)
                {
                    if (checkpoint == _entriedCheckpoints[_entriedCheckpoints.Length - 1])
                    {
                        CountStop(triggerClock);
                        return;
                    }
                }

                // 周回モード
                if (checkpoint == _entriedCheckpoints[0])
                {
                    CountLap(triggerClock);
                    if (_latestLap >= _entriedNumberOfLaps)
                    {
                        CountStop(triggerClock);
                        return;
                    }
                }

                _nextCheckpoint = GetNextCheckpoint(checkpoint);
                return;
            }

            var course = checkpoint.course;
            if (!course) { return; }
            var checkpoints = course.checkpoints;
            if (checkpoints.Length < 1) { return; }

            // コース出場処理
            if (checkpoint == checkpoints[0])
            {
                CourseEntry(course);
                CountStart(triggerClock);
                return;
            }
        }

        internal void SetDriver(VRCPlayerApi driver)
        {
            if (!Utilities.IsValid(driver)) { return; }

            if (_driver != driver)
            {
                _driver = driver;

                if (driver.isLocal && driver != Networking.GetOwner(this.gameObject))
                {
                    Networking.SetOwner(driver, this.gameObject);
                }

                CountReset();
            }
        }

        private void CourseEntry(CourseDescriptor course)
        {
            _entriedCourse = course;
            _entriedCheckpoints = course.checkpoints;
            _entriedNumberOfLaps = course.numberOfLaps;
            personalRecord = course.localPersonalRecord;
            _nextCheckpoint = GetNextCheckpoint(_entriedCheckpoints[0]);

            var sectionCount = _entriedNumberOfLaps > 0 ? _entriedNumberOfLaps * _entriedCheckpoints.Length : _entriedCheckpoints.Length - 1;
            _stopwatch.CountReset(sectionCount);

            RequestSerialization();
        }

        private Checkpoint GetNextCheckpoint(Checkpoint currentCheckpoint)
        {
            var current = Array.IndexOf(_entriedCheckpoints, currentCheckpoint);
            if (current < 0) { return null; }

            return (current < _entriedCheckpoints.Length - 1) ? _entriedCheckpoints[current + 1] : _entriedCheckpoints[0];
        }

        private void CountReset()
        {
            _entriedCourse = null;
            _entriedCheckpoints = new Checkpoint[0];
            _entriedNumberOfLaps = 0;
            personalRecord = null;
            _nextCheckpoint = null;

            _stopwatch.CountReset(0);

            _latestSection = 0;
            _latestLap = 0;
            _latestSectionTime = _GetSectionTime(_latestSection);
            _latestSplitTime = _GetSplitTime(_latestSection);
            _latestLapTime = _GetLapTime(_latestLap);
            _isGoal = false;

            RequestSerialization();
        }

        private void CountStart(double triggerClock)
        {
            _stopwatch.CountStart(triggerClock);

            _latestSection = 0;
            _latestLap = 0;
            _latestSectionTime = _GetSectionTime(_latestSection);
            _latestSplitTime = _GetSplitTime(_latestSection);
            _latestLapTime = _GetLapTime(_latestLap);
            _isGoal = false;

            RequestSerialization();

            if (_speaker && _soundStart) { _speaker.PlayOneShot(_soundStart); }
        }

        private void CountSection(double triggerClock)
        {
            _stopwatch.CountLap(triggerClock);

            _latestSection++;
            _latestSectionTime = _GetSectionTime(_latestSection);
            _latestSplitTime = _GetSplitTime(_latestSection);

            RequestSerialization();

            if (_speaker && _soundCheckpoint) { _speaker.PlayOneShot(_soundCheckpoint); }
        }

        private void CountLap(double triggerClock)
        {
            _latestLap++;
            _latestLapTime = _GetLapTime(_latestLap);

            RequestSerialization();
        }

        private void CountStop(double triggerClock)
        {
            _nextCheckpoint = null;

            _stopwatch.CountStop(triggerClock);
            _isGoal = true;

            if (_speaker && _soundGoal) { _speaker.PlayOneShot(_soundGoal); }
        }
    }
}
