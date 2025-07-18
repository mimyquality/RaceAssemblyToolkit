﻿/*
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
    public class CourseDescriptor : UdonSharpBehaviour
    {
        [Header("Course Settings")]
        public string courseName = "";
        
        [SerializeField]
        internal int revision = 1;
        [SerializeField]
        internal Checkpoint[] checkpoints = new Checkpoint[0];
        [SerializeField, Min(0)]
        internal int numberOfLaps = 0;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        internal float recordOverCut = 0.0f;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        internal float recordUnderCut = float.MaxValue;

        [Header("Participate Runners")]
        [SerializeField]
        private RaceRunner[] _runners = new RaceRunner[0];
        [SerializeField]
        private RaceRunnerAsPlayer _runnerAsPlayer;
        [SerializeField]
        private RaceRunnerAsDrone _runnerAsDrone;

        internal PlayerRecord localPersonalRecord;

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
