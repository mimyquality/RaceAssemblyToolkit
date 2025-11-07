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

    //using VRC.SDKBase;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Course Descriptor")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1000)]
    public class CourseDescriptor : UdonSharpBehaviour
    {
        private const int PlayerCacheCapacity = 90;

        [Header("Course Settings")]
        [SerializeField]
        private string _courseName = "";
        [SerializeField]
        private int _revision = 1;
        [SerializeField]
        internal Checkpoint[] checkpoints = new Checkpoint[0];
        [SerializeField, Min(0)]
        private int _numberOfLaps = 0;

        [Header("Records")]
        [SerializeField]
        private RaceRecord _raceRecord;
        [SerializeField]
        private CourseRecord _courseRecord;
        [SerializeField]
        private PersonalRecord _personalRecord;

        [Space]
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _recordOverCut = 0.0f;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _recordUnderCut = float.MaxValue;

        [Header("Participate Runners")]
        [SerializeField]
        private RaceRunner[] _runners = new RaceRunner[0];

        [SerializeField, HideInInspector]
        private RaceRunnerAsPlayer _runnerAsPlayer;
        [SerializeField, HideInInspector]
        private RaceRunnerAsDrone _runnerAsDrone;

        internal RaceRecord localRaceRecord;
        internal CourseRecord localCourseRecord;
        internal PersonalRecord localPersonalRecord;

        public string CourseName { get => _courseName; }
        public int Revision { get => _revision; }
        public int NumberOfLaps { get => _numberOfLaps; }
        public RaceRecord RaceRecord { get => _raceRecord; }
        public CourseRecord CourseRecord { get => _courseRecord; }
        public PersonalRecord PersonalRecord { get => _personalRecord; }
        public float RecordOverCut { get => _recordOverCut; }
        public float RecordUnderCut { get => _recordUnderCut; }
        public RaceRunner[] Runners { get => _runners; }
        public RaceRunnerAsPlayer RunnerAsPlayer { get => _runnerAsPlayer; }
        public RaceRunnerAsDrone RunnerAsDrone { get => _runnerAsDrone; }

        private VRCPlayerApi[] _players = new VRCPlayerApi[PlayerCacheCapacity];
        private RaceRunner[][] _playersRunners = new RaceRunner[PlayerCacheCapacity][];
        private RaceRunner[] _runnersEmpty = new RaceRunner[0];

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            _runnerAsPlayer = null;
            _runnerAsDrone = null;
            foreach (var runner in _runners)
            {
                var type = runner.GetType();
                if (!_runnerAsPlayer && type.IsSubclassOf(typeof(RaceRunnerAsPlayer)))
                {
                    _runnerAsPlayer = (RaceRunnerAsPlayer)runner;
                }
                if (!_runnerAsDrone && type.IsSubclassOf(typeof(RaceRunnerAsDrone)))
                {
                    _runnerAsDrone = (RaceRunnerAsDrone)runner;
                }
                if (_runnerAsPlayer && _runnerAsDrone) { break; }
            }

            if (_raceRecord && _raceRecord.course != this)
            {
                _raceRecord.course = this;
            }
            if (_courseRecord && _courseRecord.course != this)
            {
                _courseRecord.course = this;
            }
            if (_personalRecord && _personalRecord.course != this)
            {
                _personalRecord.course = this;
            }
        }
#endif

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            for (int i = 0; i < checkpoints.Length; i++)
            {
                checkpoints[i].course = this;
                checkpoints[i].participatingRunnerAsPlayer = _runnerAsPlayer;
                checkpoints[i].participatingRunnerAsDrone = _runnerAsDrone;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshParticipatingRunners), 1);
        }
        public void _RefreshParticipatingRunners()
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (Utilities.IsValid(_players[i])) { continue; }

                _players[i] = null;
                _playersRunners[i] = _runnersEmpty;
            }
        }

        public RaceRunner[] GetParticipatingRunners(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) { return _runnersEmpty; }

            var index = System.Array.IndexOf(_players, player);
            if (index < 0)
            {
                index = ParticipateRunners(player);
                if (index < 0) { return _runnersEmpty; }
            }

            return _playersRunners[index];
        }

        private int ParticipateRunners(VRCPlayerApi player)
        {
            var result = System.Array.IndexOf(_players, null);

            if (result < 0)
            {
                _RefreshParticipatingRunners();

                result = System.Array.IndexOf(_players, null);
                if (result < 0) { return result; }
            }

            var playerRunners = new RaceRunner[_runners.Length];
            for (int i = 0; i < playerRunners.Length; i++)
            {
                playerRunners[i] = (RaceRunner)player.FindComponentInPlayerObjects(_runners[i]);
            }

            _players[result] = player;
            _playersRunners[result] = playerRunners;

            return result;
        }
    }
}
