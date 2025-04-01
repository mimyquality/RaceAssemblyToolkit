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
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Runner as Drone")]
    [RequireComponent(typeof(VRCPlayerObject), typeof(RaceDriver))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RaceRunnerAsDrone : RaceRunner
    {
        private void Reset()
        {
            var raceDriver = GetComponent<RaceDriver>();
            raceDriver.targetRunner = this;

            runnerName = "Drone";
        }
    }
}
