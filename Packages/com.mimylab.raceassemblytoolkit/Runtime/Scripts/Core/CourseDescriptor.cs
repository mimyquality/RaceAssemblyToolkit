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
        public string courseName = "";

        [SerializeField]
        private Checkpoint[] _checkpoints = new Checkpoint[0];
        [SerializeField, Min(0)]
        private int _lapCount = 0;
        [SerializeField, Min(0.0f)]
        private float _recordOverCut = 0.0f;

        [Header("Additional Settings")]
        [SerializeField]
        private RaceDriverAsPlayer _raceDriverAsPlayer;

        internal PlayerRecord localPlayerRecord;

        public int LapCount { get => _lapCount; }
        public Checkpoint[] Checkpoints { get => _checkpoints; }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            for (int i = 0; i < _checkpoints.Length; i++)
            {
                _checkpoints[i].course = this;
                _checkpoints[i].driverAsPlayerTemplate = _raceDriverAsPlayer;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }
    }
}
