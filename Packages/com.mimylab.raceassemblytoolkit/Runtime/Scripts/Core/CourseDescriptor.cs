/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Course Descriptor")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CourseDescriptor : UdonSharpBehaviour
    {
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

        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _recordOverCut = 0.0f;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _recordUnderCut = float.MaxValue;

        [Header("Participate Runners")]
        [SerializeField]
        private RaceRunner[] _runners = new RaceRunner[0];
        [SerializeField]
        private RaceRunnerAsPlayer _runnerAsPlayer;
        [SerializeField]
        private RaceRunnerAsDrone _runnerAsDrone;

        internal RaceRecord localRaceRecord;
        internal CourseRecord localCourseRecord;
        internal PersonalRecord localPersonalRecord;

        public string CourseName { get => _courseName; }
        public int Revision { get => _revision; }
        public int NumberOfLaps { get => _numberOfLaps; }
        public float RecordOverCut { get => _recordOverCut; }
        public float RecordUnderCut { get => _recordUnderCut; }

        private void OnValidate()
        {
            if (_raceRecord && _raceRecord.course != this)
            {
                _raceRecord.course = this;
            }
            if (_raceRecord)
            {
                _raceRecord.participateRunners = _runners;
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

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            for (int i = 0; i < checkpoints.Length; i++)
            {
                checkpoints[i].course = this;
                checkpoints[i].participateRunners = _runners;
                checkpoints[i].participateRunnerAsPlayer = _runnerAsPlayer;
                checkpoints[i].participateRunnerAsDrone = _runnerAsDrone;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }
    }
}
