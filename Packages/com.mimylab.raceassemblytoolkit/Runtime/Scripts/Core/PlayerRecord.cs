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

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Player Record")]
    [RequireComponent(typeof(VRCPlayerObject), typeof(VRCEnablePersistence))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerRecord : UdonSharpBehaviour
    {
        [SerializeField]
        internal CourseDescriptor course;

        internal RaceRunner[] entryRunners = new RaceRunner[0];
        internal RaceRunner[] entryRunnersAs = new RaceRunner[0];

        private TimeSpan _bestRecordTime;
        private string _bestRecordRunnerVariety = "";

        private TimeSpan _recordTime;
        private RaceRunner _recordRunner;
        private string _recordRunnerVariety = "";

        private TimeSpan[] _historyRecordTime = new TimeSpan[0];
        private int[] _historyRunnerNumber = new int[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (Networking.IsOwner(this.gameObject))
            {
                course.localPlayerRecord = this;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public bool AddRedcordTime(TimeSpan recordTime, RaceRunner runner)
        {
            if (!Networking.IsOwner(this.gameObject)) { return false; }

            var index = Array.IndexOf(entryRunnersAs, runner);
            if (index > -1)
            {
                index = entryRunners.Length + index;
                AddRecordTimeToHistory(recordTime, index);
                return true;
            }

            index = Array.IndexOf(entryRunners, runner);
            if (index > -1)
            {
                AddRecordTimeToHistory(recordTime, index);
                return true;
            }

            return false;
        }

        private void AddRecordTimeToHistory(TimeSpan recordTime, int runnerNumber)
        {
            var newHistoryRecordTime = new TimeSpan[_historyRecordTime.Length + 1];
            var newHistoryRunnerNumber = new int[_historyRunnerNumber.Length + 1];

            _historyRecordTime.CopyTo(newHistoryRecordTime, 0);
            _historyRunnerNumber.CopyTo(newHistoryRunnerNumber, 0);

            newHistoryRecordTime[newHistoryRecordTime.Length - 1] = recordTime;
            newHistoryRunnerNumber[newHistoryRunnerNumber.Length - 1] = runnerNumber;

            _historyRecordTime = newHistoryRecordTime;
            _historyRunnerNumber = newHistoryRunnerNumber;
        }

        private void InsertRecordTimeToHistory(TimeSpan recordTime, int runnerNumber)
        {
            var newHistoryRecordTime = new TimeSpan[_historyRecordTime.Length + 1];
            var newHistoryRunnerNumber = new int[_historyRunnerNumber.Length + 1];

            for (int i = 0; i < _historyRecordTime.Length; i++)
            {
                if (_historyRecordTime[i] > recordTime)
                {
                    Array.Copy(_historyRecordTime, 0, newHistoryRecordTime, 0, i);
                    Array.Copy(_historyRunnerNumber, 0, newHistoryRunnerNumber, 0, i);

                    newHistoryRecordTime[i] = recordTime;
                    newHistoryRunnerNumber[i] = runnerNumber;

                    Array.Copy(_historyRecordTime, i, newHistoryRecordTime, i + 1, _historyRecordTime.Length - i);
                    Array.Copy(_historyRunnerNumber, i, newHistoryRunnerNumber, i + 1, _historyRecordTime.Length - i);

                    break;
                }

                if (i > _historyRecordTime.Length - 2)
                {
                    Array.Copy(_historyRecordTime, 0, newHistoryRecordTime, 0, _historyRecordTime.Length);
                    Array.Copy(_historyRunnerNumber, 0, newHistoryRunnerNumber, 0, _historyRecordTime.Length);

                    newHistoryRecordTime[i + 1] = recordTime;
                    newHistoryRunnerNumber[i + 1] = runnerNumber;

                    break;
                }
            }
            _historyRecordTime = newHistoryRecordTime;
            _historyRunnerNumber = newHistoryRunnerNumber;
        }
    }
}
