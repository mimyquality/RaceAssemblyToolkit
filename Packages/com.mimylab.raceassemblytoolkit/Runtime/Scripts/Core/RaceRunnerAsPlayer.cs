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
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Runner as Player")]
    [RequireComponent(typeof(VRCPlayerObject), typeof(RaceDriver))]
    public class RaceRunnerAsPlayer : RaceRunner
    {
        private void Reset()
        {
            _raceDriver = GetComponent<RaceDriver>();
            _raceDriver.raceRunner = this;

            _variety = "Player";
        }

        protected override void UpdateRecord()
        {
            if (_raceRecord) { _raceRecord.OnRunnerAsPlayerUpdate(this); }
            //if (_courseRecord) { _courseRecord.OnRunnerAsPlayerUpdate(this); }
            //if (_personalRecord) { _personalRecord.OnRunnerAsPlayerUpdate(this); }
        }
    }
}
