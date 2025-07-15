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

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Stopwatch")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Stopwatch : UdonSharpBehaviour
    {
        [SerializeField, Min(1)]
        private int _maxLaps = 99;

        private int _lap = 0;
        private bool _isCounting = false;
        private double[] _clocks = new double[1];
        private double _currentClock = 0.0d;

        public int MaxLaps { get => _maxLaps; }
        public int Lap { get => _lap; }
        public bool IsCounting { get => _isCounting; }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            CountReset();

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public void CountReset()
        {
            CountReset(_maxLaps);
        }
        public void CountReset(int maxLaps)
        {
            _isCounting = false;

            _maxLaps = maxLaps;
            _clocks = new double[maxLaps + 1];
            _lap = 0;
            _currentClock = 0.0d;
        }

        public void CountStart()
        {
            CountStart(Time.timeAsDouble);
        }
        public void CountStart(double triggerClock)
        {
            Initialize();

            if (_isCounting) { return; }

            _isCounting = true;

            if (_clocks[0] == 0.0d)
            {
                _clocks[0] = triggerClock;
            }
            else
            {
                for (int i = 0; i <= _lap; i++)
                {
                    _clocks[i] += triggerClock - _currentClock;
                }
            }
        }

        public void CountLap()
        {
            CountLap(Time.timeAsDouble);
        }
        public void CountLap(double triggerClock)
        {
            Initialize();

            if (!_isCounting) { return; }
            if (_lap >= _maxLaps) { return; }

            _clocks[++_lap] = triggerClock;
        }

        public void CountStop()
        {
            CountStop(Time.timeAsDouble);
        }
        public void CountStop(double triggerClock)
        {
            Initialize();

            if (!_isCounting) { return; }

            _isCounting = false;
            _currentClock = triggerClock;
        }

        public TimeSpan GetLapTime(int lap)
        {
            if (lap < 1) { return TimeSpan.Zero; }
            if (lap >= _clocks.Length) { return TimeSpan.Zero; }
            if (_clocks[lap] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_clocks[lap] - _clocks[lap - 1]);
        }

        public TimeSpan GetSplitTime(int lap)
        {
            if (lap < 1) { return TimeSpan.Zero; }
            if (lap >= _clocks.Length) { return TimeSpan.Zero; }
            if (_clocks[lap] == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_clocks[lap] - _clocks[0]);
        }

        public TimeSpan GetTotalTime()
        {
            if (_isCounting) { return TimeSpan.Zero; }
            if (_clocks[0] == 0.0d) { return TimeSpan.Zero; }
            if (_currentClock == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_currentClock - _clocks[0]);
        }

        public TimeSpan GetCurrentTime()
        {
            if (_clocks[0] == 0.0d) { return TimeSpan.Zero; }

            if (_isCounting) { _currentClock = Time.timeAsDouble; }
            if (_currentClock == 0.0d) { return TimeSpan.Zero; }

            return TimeSpan.FromSeconds(_currentClock - _clocks[0]);
        }
    }
}
