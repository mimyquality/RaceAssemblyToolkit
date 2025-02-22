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
    [AddComponentMenu("Race Assembly Toolkit/Race Driver as Player")]
    [RequireComponent(typeof(VRCPlayerObject))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RaceDriverAsPlayer : RaceDriver
    {
        private void Start()
        {
            SendCustomEventDelayedFrames(nameof(_SetDriver), 1);
        }
    }
}
