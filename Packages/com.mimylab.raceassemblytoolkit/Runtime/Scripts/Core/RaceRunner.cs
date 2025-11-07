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
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Runner")]
    [RequireComponent(typeof(Stopwatch))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RaceRunner : UdonSharpBehaviour
    {
        [Header("Require References")]
        [SerializeField]
        protected RaceDriver _raceDriver;

        [Header("Base Settings")]
        protected string _variety = "";

        [Header("Additional Settings")]
        [SerializeField]
        private AudioSource _speaker;
        [SerializeField]
        private AudioClip _soundStart;
        [SerializeField]
        private AudioClip _soundCheckpoint;
        [SerializeField]
        private AudioClip _soundGoal;

        protected RaceRecord _raceRecord;
        protected CourseRecord _courseRecord;
        protected PersonalRecord _personalRecord;

        private Stopwatch _stopwatch;
        private VRCPlayerApi _driver;
        private CourseDescriptor _entriedCourse;
        private int _entriedNumberOfLaps;
        private Checkpoint[] _entriedCheckpoints = new Checkpoint[0];
        private Checkpoint _nextCheckpoint;
        private bool _isEntry;
        private bool _isGoal;
        private int _latestSection;
        private int _latestLap;
        private TimeSpan _latestSectionTime;
        private TimeSpan _latestSplitTime;
        private TimeSpan _latestLapTime;

        public string Variety { get => _variety; }
        public CourseDescriptor EntriedCourse { get => _entriedCourse; }
        public bool IsEntry { get => _isEntry; }
        public bool IsGoal { get => _isGoal; }
        public int NumberOfLaps { get => _entriedNumberOfLaps; }
        public int LatestSection { get => _latestSection; }
        public int LatestLap { get => _entriedNumberOfLaps > 0 ? _latestLap : _latestSection; }
        public TimeSpan LatestSectionTime { get => _latestSectionTime; }
        public TimeSpan LatestLapTime { get => _latestLapTime; }
        public TimeSpan LatestSplitTime { get => _latestSplitTime; }
        public TimeSpan GoalTime { get => _isGoal ? _latestSplitTime : TimeSpan.Zero; }

        private Stopwatch _Stopwatch { get => _stopwatch ? _stopwatch : GetComponent<Stopwatch>(); }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        protected virtual void Reset()
        {
            var playerObject = GetComponentInParent<VRCPlayerObject>();
            if (!playerObject) { this.gameObject.AddComponent<VRCPlayerObject>(); }
        }
#endif

        private void Start()
        {
            _stopwatch = GetComponent<Stopwatch>();

            if (_raceDriver)
            {
                _raceDriver.raceRunner = this;
                _raceDriver._SetDriver();
            }
            else
            {
                Debug.LogError($"Reference Exception: RaceDriver is not assigned in {this.name}.");
            }
        }

        public VRCPlayerApi GetDriver()
        {
            return _driver;
        }

        public TimeSpan _GetSectionTime(int section)
        {
            return _Stopwatch.GetLapTime(section);
        }

        public TimeSpan _GetSplitTime(int section)
        {
            return _Stopwatch.GetSplitTime(section);
        }

        public TimeSpan _GetLapTime(int lap)
        {
            if (lap < 1) { return TimeSpan.Zero; }

            // ワンパスモードならセクション時間を返す
            if (_entriedNumberOfLaps < 1) { return _GetSectionTime(lap); }

            var section = lap * _entriedCheckpoints.Length;
            return _Stopwatch.GetLapTime(section, section - _entriedCheckpoints.Length);
        }

        public TimeSpan _GetSplitTimeByLap(int lap)
        {
            if (lap < 1) { return TimeSpan.Zero; }

            // ワンパスモードならセクション時間を返す
            if (_entriedNumberOfLaps < 1) { return _GetSplitTime(lap); }

            var section = lap * _entriedCheckpoints.Length;
            return _Stopwatch.GetSplitTime(section);
        }

        public TimeSpan _GetTotalTime()
        {
            return _Stopwatch.GetTotalTime();
        }

        public TimeSpan _GetCurrentTime()
        {
            return _Stopwatch.GetCurrentTime();
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

                        UpdateRecord();
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

                        UpdateRecord();
                        return;
                    }
                }

                _nextCheckpoint = GetNextCheckpoint(checkpoint);

                UpdateRecord();
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

                UpdateRecord();
                return;
            }
        }

        internal void SetDriver(VRCPlayerApi driver)
        {
            if (!Utilities.IsValid(driver)) { return; }

            if (_driver != driver)
            {
                _driver = driver;

                CountReset();

                UpdateRecord();
            }
        }

        protected virtual void UpdateRecord()
        {
            if (!_driver.isLocal) { return; }

            if (_raceRecord) { _raceRecord.OnRunnerUpdate(this); }
            //if (_courseRecord) {_courseRecord.OnRunnerUpdate(this); }
            //if (_personalRecord) { _personalRecord.OnRunnerUpdate(this); }
        }

        private void CourseEntry(CourseDescriptor course)
        {
            _entriedCourse = course;
            _entriedNumberOfLaps = course.NumberOfLaps;
            _entriedCheckpoints = course.checkpoints;
            _raceRecord = course.localRaceRecord;
            _courseRecord = course.localCourseRecord;
            _personalRecord = course.localPersonalRecord;
            _nextCheckpoint = GetNextCheckpoint(_entriedCheckpoints[0]);

            var sectionCount = _entriedNumberOfLaps > 0 ? _entriedNumberOfLaps * _entriedCheckpoints.Length : _entriedCheckpoints.Length - 1;
            _Stopwatch.CountReset(sectionCount);

            _isEntry = true;
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
            _entriedNumberOfLaps = 0;
            _entriedCheckpoints = new Checkpoint[0];
            _raceRecord = null;
            _courseRecord = null;
            _personalRecord = null;
            _nextCheckpoint = null;

            _Stopwatch.CountReset(0);

            _isEntry = false;
            _isGoal = false;
            _latestSection = 0;
            _latestLap = 0;
            _latestSectionTime = _GetSectionTime(_latestSection);
            _latestSplitTime = _GetSplitTime(_latestSection);
            _latestLapTime = _GetLapTime(_latestLap);

        }

        private void CountStart(double triggerClock)
        {
            _Stopwatch.CountStart(triggerClock);

            _latestSection = 0;
            _latestLap = 0;
            _latestSectionTime = _GetSectionTime(_latestSection);
            _latestSplitTime = _GetSplitTime(_latestSection);
            _latestLapTime = _GetLapTime(_latestLap);
            _isGoal = false;

            if (_speaker && _soundStart) { _speaker.PlayOneShot(_soundStart); }
        }

        private void CountSection(double triggerClock)
        {
            _Stopwatch.CountLap(triggerClock);

            _latestSection++;
            _latestSectionTime = _GetSectionTime(_latestSection);
            _latestSplitTime = _GetSplitTime(_latestSection);

            if (_speaker && _soundCheckpoint) { _speaker.PlayOneShot(_soundCheckpoint); }
        }

        private void CountLap(double triggerClock)
        {
            _latestLap++;
            _latestLapTime = _GetLapTime(_latestLap);
        }

        private void CountStop(double triggerClock)
        {
            _nextCheckpoint = null;

            _Stopwatch.CountStop(triggerClock);
            _isGoal = true;

            if (_speaker && _soundGoal) { _speaker.PlayOneShot(_soundGoal); }
        }
    }
}
