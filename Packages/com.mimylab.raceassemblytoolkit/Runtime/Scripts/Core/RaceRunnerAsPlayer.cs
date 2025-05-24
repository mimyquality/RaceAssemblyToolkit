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
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RaceRunnerAsPlayer : RaceRunner
    {
        private void Reset()
        {
            var raceDriver = GetComponent<RaceDriver>();
            raceDriver.targetRunner = this;
            
            variety = "Player";
        }
    }
}
