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
    using VRC.Udon;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Ranking Board")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RaceRankingBoard : IRecordReceiver
    {
        [SerializeField]
        private CourseDescriptor _course;
        [SerializeField]
        private RaceRankingPlate _plateTemplate;

        private RaceRunner[] _participateRunners = new RaceRunner[0];
        private RaceRunnerAsPlayer _participateRunnerAsPlayer;
        private RaceRunnerAsDrone _participateRunnerAsDrone;

        private RaceRecord _raceRecord;

        private Transform _plateParent;
        private RaceRankingPlate[] _plates = new RaceRankingPlate[0];
        private RaceRunner[] _runners = new RaceRunner[0];
        private TimeSpan[] _records = new TimeSpan[0];
        private int[] _latestSections = new int[0];
        private int[] _ranking = new int[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _plateParent = _plateTemplate.transform.parent;
            _plateTemplate.gameObject.SetActive(false);

            _raceRecord = _course.RaceRecord;

            _participateRunners = _course.Runners;
            _participateRunnerAsPlayer = _course.RunnerAsPlayer;
            _participateRunnerAsDrone = _course.RunnerAsDrone;

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            ClearRunners();
            AddRunners(_participateRunners);

            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
            for (int i = 0; i < players.Length; i++)
            {
                SetupFromPlayerRunner(players[i]);
            }

            RefreshPlatesAll();
            InsertSortByRecord();
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            Initialize();

            SetupFromPlayerRunner(player);

            RefreshPlatesAll();
            InsertSortByRecord();
        }

        internal override void OnRaceRecordUpdate(RaceRecord record)
        {
            Initialize();

            // record を元にプレート更新

            InsertSortByRecord();
        }

        private void SetupFromPlayerRunner(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) { return; }

            if (_participateRunnerAsPlayer)
            {
                var runnerAsPlayer = (RaceRunner)player.FindComponentInPlayerObjects(_participateRunnerAsPlayer);
                if (runnerAsPlayer)
                {
                    AddRunner(runnerAsPlayer);
                }
            }

            if (_participateRunnerAsDrone)
            {
                var runnerAsDrone = (RaceRunner)player.FindComponentInPlayerObjects(_participateRunnerAsDrone);
                if (runnerAsDrone)
                {
                    AddRunner(runnerAsDrone);
                }
            }

            var raceRecord = (RaceRecord)player.FindComponentInPlayerObjects(_raceRecord);
            if (raceRecord)
            {
                raceRecord.AddRecordReveiver(this);
            }
        }

        private void AddRunner(RaceRunner runner)
        {
            if (!runner) { return; }

            AddRunners(new RaceRunner[] { runner });
        }

        private void AddRunners(RaceRunner[] runners)
        {
            Initialize();

            if (runners.Length < 1) { return; }

            var platesCount = _plates.Length;
            var platesEnd = platesCount + runners.Length;
            var tmpPlates = new RaceRankingPlate[platesEnd];
            var tmpRunners = new RaceRunner[platesEnd];
            var tmpRecords = new TimeSpan[platesEnd];
            var tmpLatestSections = new int[platesEnd];
            var tmpRanking = new int[platesEnd];

            Array.Copy(_plates, tmpPlates, platesCount);
            Array.Copy(_runners, tmpRunners, platesCount);
            Array.Copy(_records, tmpRecords, platesCount);
            Array.Copy(_latestSections, tmpLatestSections, platesCount);
            Array.Copy(_ranking, tmpRanking, platesCount);
            for (int i = platesCount; i < platesEnd; i++)
            {
                var plateObject = Instantiate(_plateTemplate.gameObject, _plateParent);
                tmpPlates[i] = plateObject.GetComponent<RaceRankingPlate>();
                tmpRunners[i] = runners[i - platesCount];
                tmpRecords[i] = tmpRunners[i].LatestSplitTime;
                tmpLatestSections[i] = tmpRunners[i].LatestSection;
            }

            _plates = tmpPlates;
            _runners = tmpRunners;
            _records = tmpRecords;
            _latestSections = tmpLatestSections;
            _ranking = tmpRanking;
        }

        private void RemoveRunner(RaceRunner runner)
        {
            Initialize();

            if (!runner) { return; }

            var index = Array.IndexOf(_runners, runner);
            if (index < 0) { return; }

            var tmpPlates = new RaceRankingPlate[_plates.Length - 1];
            Array.Copy(_plates, 0, tmpPlates, 0, index);
            Array.Copy(_plates, index + 1, tmpPlates, index, _plates.Length - index - 1);
            _plates = tmpPlates;

            var tmpRunners = new RaceRunner[_runners.Length - 1];
            Array.Copy(_runners, 0, tmpRunners, 0, index);
            Array.Copy(_runners, index + 1, tmpRunners, index, _runners.Length - index - 1);
            _runners = tmpRunners;

            Destroy(_plates[index].gameObject);
        }

        private void ClearRunners()
        {
            Initialize();

            _plates = new RaceRankingPlate[0];
            _runners = new RaceRunner[0];
            _records = new TimeSpan[0];
            _latestSections = new int[0];
            _ranking = new int[0];

            foreach (Transform child in _plateParent)
            {
                if (child == _plateTemplate.transform) { continue; }

                Destroy(child.gameObject);
            }
        }

        private void RefreshPlatesAll()
        {

        }

        private void InsertSortByRecord()
        {
            if (_plates.Length < 2) { return; }

            var tmpIndex = new int[_plates.Length];
            for (int i = 1; i < _plates.Length; i++)
            {
                tmpIndex[i] = i;
                for (int j = i; j > 0; j--)
                {
                    if (_latestSections[tmpIndex[j - 1]] < _latestSections[i]) { continue; }

                    if (_latestSections[tmpIndex[j - 1]] == _latestSections[i] &&
                       _records[tmpIndex[j - 1]] > _records[i])
                    {
                        continue;
                    }

                    Array.Copy(tmpIndex, j, tmpIndex, j + 1, i - j);
                    tmpIndex[j] = i;
                    break;
                }
            }

            var tmpPlates = new RaceRankingPlate[_plates.Length];
            var tmpRunners = new RaceRunner[_plates.Length];
            var tmpRecords = new TimeSpan[_plates.Length];
            var tmpLatestSections = new int[_plates.Length];
            for (int i = 0; i < _plates.Length; i++)
            {
                tmpPlates[i] = _plates[tmpIndex[i]];
                tmpRunners[i] = _runners[tmpIndex[i]];
                tmpRecords[i] = _records[tmpIndex[i]];
                tmpLatestSections[i] = _latestSections[tmpIndex[i]];
                _ranking[i] = (i > 0 && tmpRecords[i - 1] == tmpRecords[i]) ? _ranking[i - 1] : i + 1;

            }
            tmpPlates.CopyTo(_plates, 0);
            tmpRunners.CopyTo(_runners, 0);
            tmpRecords.CopyTo(_records, 0);
            tmpLatestSections.CopyTo(_latestSections, 0);
        }
    }
}
