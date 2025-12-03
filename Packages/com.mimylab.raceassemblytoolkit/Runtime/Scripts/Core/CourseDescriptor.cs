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

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Course Descriptor")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1000)]
    public class CourseDescriptor : UdonSharpBehaviour
    {
        public const int MaxPlayerCount = 90;
        public const int ExtendPlayerCount = 30;

        [Header("Course Settings")]
        [SerializeField]
        private string _courseName = "";
        [SerializeField, Min(0)]
        private int _numberOfLaps = 0;
        [SerializeField]
        internal Checkpoint[] checkpoints = new Checkpoint[0];
        [SerializeField]
        internal RaceRunner[] raceRunners = new RaceRunner[0];

        [Header("Records")]
        [SerializeField]
        internal RaceRecord raceRecord;
        [SerializeField]
        internal CourseRecord courseRecord;
        [SerializeField]
        internal PersonalRecord personalRecord;

        [Space]
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _recordOverCut = 0.0f;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _recordUnderCut = float.MaxValue;

        [SerializeField, HideInInspector]
        internal RaceRunnerAsPlayer runnerAsPlayer;
        [SerializeField, HideInInspector]
        internal RaceRunnerAsDrone runnerAsDrone;

        internal RaceRecord localRaceRecord;
        internal CourseRecord localCourseRecord;
        internal PersonalRecord localPersonalRecord;

        public string CourseName { get => _courseName; }
        public int NumberOfLaps { get => _numberOfLaps; }
        public float RecordOverCut { get => _recordOverCut; }
        public float RecordUnderCut { get => _recordUnderCut; }
        /* 
        public RaceRunner[] Runners { get => _runners; }
        public RaceRunnerAsPlayer RunnerAsPlayer { get => runnerAsPlayer; }
        public RaceRunnerAsDrone RunnerAsDrone { get => runnerAsDrone; }
         */

        private VRCPlayerApi[] _players = new VRCPlayerApi[MaxPlayerCount];
        private RaceRunner[][] _playersRunners = new RaceRunner[MaxPlayerCount][];
        private RaceRunner[] _runnersEmpty = new RaceRunner[0];

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            runnerAsPlayer = null;
            runnerAsDrone = null;
            foreach (var runner in raceRunners)
            {
                var type = runner.GetType();
                if (!runnerAsPlayer && type.IsSubclassOf(typeof(RaceRunnerAsPlayer)))
                {
                    runnerAsPlayer = (RaceRunnerAsPlayer)runner;
                }
                if (!runnerAsDrone && type.IsSubclassOf(typeof(RaceRunnerAsDrone)))
                {
                    runnerAsDrone = (RaceRunnerAsDrone)runner;
                }
                if (runnerAsPlayer && runnerAsDrone) { break; }
            }

            if (raceRecord && raceRecord.course != this)
            {
                raceRecord.course = this;
            }
            if (courseRecord && courseRecord.course != this)
            {
                courseRecord.course = this;
            }
            if (personalRecord && personalRecord.course != this)
            {
                personalRecord.course = this;
            }
        }
#endif

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            for (int i = 0; i < checkpoints.Length; i++)
            {
                checkpoints[i].SetCourse(this);
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
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
                for (int i = 0; i < _players.Length; i++)
                {
                    if (Utilities.IsValid(_players[i])) { continue; }

                    _players[i] = null;
                    _playersRunners[i] = _runnersEmpty;
                }

                result = System.Array.IndexOf(_players, null);
                // 全プレイヤー有効＝インスタンスに居る　なので上限拡張
                if (result < 0)
                {
                    result = _players.Length;
                    var tmp_players = new VRCPlayerApi[_players.Length + ExtendPlayerCount];
                    var tmp_playersRunners = new RaceRunner[_playersRunners.Length + ExtendPlayerCount][];
                    _players.CopyTo(tmp_players, 0);
                    _playersRunners.CopyTo(tmp_playersRunners, 0);
                    _players = tmp_players;
                    _playersRunners = tmp_playersRunners;
                }
            }

            var playerRunners = new RaceRunner[raceRunners.Length];
            for (int i = 0; i < playerRunners.Length; i++)
            {
                playerRunners[i] = (RaceRunner)player.FindComponentInPlayerObjects(raceRunners[i]);
            }

            _players[result] = player;
            _playersRunners[result] = playerRunners;

            return result;
        }
    }
}
