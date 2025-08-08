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
    public class RaceRankingBoard : UdonSharpBehaviour
    {
        [SerializeField]
        private RaceRankingPlate _plateTemplate;

        internal CourseDescriptor course;

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

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        internal void ResetPlates()
        {
            Initialize();

            _plates = new RaceRankingPlate[0];
            _runners = new RaceRunner[0];
            _records = new TimeSpan[0];
            _latestSections = new int[0];
            _ranking = new int[0];

            foreach (Transform child in _plateParent)
            {
                if (child != _plateTemplate.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        internal void AddPlates(RaceRunner[] runners)
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

            InsertSortByRecord();

            for (int i = platesCount; i < platesEnd; i++)
            {
                OnRunnerUpdated(_runners[i]);
            }
        }

        internal void AddPlate(RaceRunner runner)
        {
            Initialize();

            if (!runner) { return; }
            if (Array.IndexOf(_runners, runner) > -1) { return; }

            var plateObject = Instantiate(_plateTemplate.gameObject, _plateParent);
            var plate = plateObject.GetComponent<RaceRankingPlate>();

            var tmpPlates = new RaceRankingPlate[_plates.Length + 1];
            Array.Copy(_plates, tmpPlates, _plates.Length);
            tmpPlates[_plates.Length] = plate;
            _plates = tmpPlates;

            var tmpRunners = new RaceRunner[_runners.Length + 1];
            Array.Copy(_runners, tmpRunners, _runners.Length);
            tmpRunners[_runners.Length] = runner;
            _runners = tmpRunners;

            plate.Course = course;
            plate.Driver = runner.GetDriver();
            plate.Section = runner.LatestSection;
            plate.Lap = runner.LatestLap;
            plate.SectionTime = runner.LatestSectionTime;
            plate.SplitTime = runner.LatestSplitTime;
            plate.LapTime = runner.LatestLapTime;
        }

        internal void RemovePlate(RaceRunner runner)
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

        internal void OnRunnerUpdated(RaceRunner runner)
        {
            Initialize();

            if (!runner) { return; }

            var index = Array.IndexOf(_runners, runner);
            if (index < 0) { return; }

            var plate = _plates[index];
            plate.Driver = runner.GetDriver();
            plate.Section = runner.LatestSection;
            plate.Lap = runner.LatestLap;
            plate.SectionTime = runner.LatestSectionTime;
            plate.SplitTime = runner.LatestSplitTime;
            plate.LapTime = runner.LatestLapTime;
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

        private void MoveUpPlate(RaceRankingPlate plate)
        {
            Initialize();

            if (!plate) { return; }

            var index = Array.IndexOf(_plates, plate);
            if (index < 0) { return; }


        }
    }
}
