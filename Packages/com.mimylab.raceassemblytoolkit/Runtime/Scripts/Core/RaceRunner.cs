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

        internal PlayerRecord playerRecord;

        [UdonSynced]
        int sync_numberOfLaps;
        [UdonSynced]
        int sync_latestSection;
        [UdonSynced]
        int sync_latestLap;
        [UdonSynced]
        long sync_latestSectionTime;
        [UdonSynced]
        long sync_latestSplitTime;
        [UdonSynced]
        long sync_latestLapTime;

        private VRCPlayerApi _driver;
        private CourseDescriptor _entriedCourse;
        private Checkpoint[] _entriedCheckpoints = new Checkpoint[0];
        private int _entriedNumberOfLaps;
        private double[] _sectionClocks = new double[1];
        private Checkpoint _nextCheckpoint;
        private int _latestSection;
        private int _latestLap;
        private TimeSpan _latestSectionTime;
        private TimeSpan _latestSplitTime;
        private TimeSpan _latestLapTime;

        public CourseDescriptor EntriedCourse { get => _entriedCourse; }
        public int NumberOfLaps { get => _entriedNumberOfLaps; }
        public TimeSpan CurrentTime { get => (_sectionClocks[0] == 0.0d) ? TimeSpan.Zero : TimeSpan.FromSeconds(Time.timeAsDouble - _sectionClocks[0]); }
        public TimeSpan TotalTime { get => GetSplitTime(_sectionClocks.Length - 1); }
        public int LatestSection { get => _latestSection; }
        public int LatestLap { get => _entriedNumberOfLaps > 0 ? _latestLap : _latestSection; }
        public TimeSpan LatestSectionTime { get => _latestSectionTime; }
        public TimeSpan LatestSplitTime { get => _latestSplitTime; }
        public TimeSpan LatestLapTime { get => _latestLapTime; }

        public override void OnPreSerialization()
        {
            sync_numberOfLaps = _entriedNumberOfLaps;
            sync_latestSection = _latestSection;
            sync_latestLap = _latestLap;
            sync_latestSectionTime = _latestSectionTime.Ticks;
            sync_latestSplitTime = _latestSplitTime.Ticks;
            sync_latestLapTime = _latestLapTime.Ticks;
        }

        public override void OnDeserialization()
        {
            _entriedNumberOfLaps = sync_numberOfLaps;
            _latestSection = sync_latestSection;
            _latestLap = sync_latestLap;
            _latestSectionTime = TimeSpan.FromTicks(sync_latestSectionTime);
            _latestSplitTime = TimeSpan.FromTicks(sync_latestSplitTime);
            _latestLapTime = TimeSpan.FromTicks(sync_latestLapTime);
        }

        public void OnCheckpointPassed(Checkpoint checkpoint, double triggerClock)
        {
            // 次のチェックポイント通過処理
            if (checkpoint == _nextCheckpoint)
            {
                CountSection(triggerClock);

                // ワンパスモード
                if (checkpoint == _entriedCheckpoints[_entriedCheckpoints.Length - 1])
                {
                    if (_entriedNumberOfLaps == 0)
                    {
                        CountStop();
                        return;
                    }
                }

                // 周回モード
                if (checkpoint == _entriedCheckpoints[0])
                {
                    CountLap();
                    if (_latestLap >= _entriedNumberOfLaps)
                    {
                        CountStop();
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

            // コース出場処理
            if (checkpoint == checkpoints[0])
            {
                Entry(course);
                CountStart(triggerClock);
                return;
            }
        }

        public VRCPlayerApi GetDriver()
        {
            return _driver;
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
            if (_entriedNumberOfLaps < 1) { return GetSectionTime(lap); }

            if (lap < 1) { return TimeSpan.Zero; }
            if (lap > _entriedNumberOfLaps) { return TimeSpan.Zero; }

            lap = lap * _entriedCheckpoints.Length;
            if (_sectionClocks[lap] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_sectionClocks[lap] - _sectionClocks[lap - _entriedCheckpoints.Length]);
        }

        public TimeSpan GetLapTimeBySectionCount(int section)
        {
            // ワンパスモードならセクション時間を返す
            if (_entriedNumberOfLaps < 1) { return GetSectionTime(section); }

            var lap = _entriedCheckpoints.Length > 0 ? section / _entriedCheckpoints.Length : 0;

            return GetLapTime(lap);
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

        private void Entry(CourseDescriptor course)
        {
            _entriedCourse = course;
            _entriedCheckpoints = course.Checkpoints;
            _entriedNumberOfLaps = course.NumberOfLaps;
            playerRecord = course.localPlayerRecord;

            var sectionCount = _entriedNumberOfLaps > 0 ? _entriedNumberOfLaps * _entriedCheckpoints.Length + 1 : _entriedCheckpoints.Length;
            _sectionClocks = new double[sectionCount];

            _nextCheckpoint = GetNextCheckpoint(_entriedCheckpoints[0]);
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
            playerRecord = null;
            _nextCheckpoint = null;
            _sectionClocks = new double[1];

            _latestSection = 0;
            _latestLap = 0;
            _latestSectionTime = GetSectionTime(_latestSection);
            _latestSplitTime = GetSplitTime(_latestSection);
            _latestLapTime = GetLapTime(_latestLap);

            RequestSerialization();
        }

        private void CountStart(double triggerClock)
        {
            _latestSection = 0;
            _latestLap = 0;
            _sectionClocks[_latestSection] = triggerClock;
            _latestSectionTime = GetSectionTime(_latestSection);
            _latestSplitTime = GetSplitTime(_latestSection);
            _latestLapTime = GetLapTime(_latestLap);

            RequestSerialization();

            if (_speaker && _soundStart) { _speaker.PlayOneShot(_soundStart); }
        }

        private void CountSection(double triggerClock)
        {
            _latestSection++;
            _sectionClocks[_latestSection] = triggerClock;

            _latestSectionTime = GetSectionTime(_latestSection);
            _latestSplitTime = GetSplitTime(_latestSection);

            RequestSerialization();

            if (_speaker && _soundCheckpoint) { _speaker.PlayOneShot(_soundCheckpoint); }
        }

        private void CountLap()
        {
            _latestLap++;
            _latestLapTime = GetLapTime(_latestLap);

            RequestSerialization();
        }

        private void CountStop()
        {
            _nextCheckpoint = null;

            if (_speaker && _soundGoal) { _speaker.PlayOneShot(_soundGoal); }
        }
    }
}
