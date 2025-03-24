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

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Driver as Object")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RaceDriverAsObject : RaceDriver
    {
        private void OnEnable()
        {
            SendCustomEventDelayedFrames(nameof(_SetDriver), 1);
        }

        private void Start()
        {
            if (!targetRunner)
            {
                targetRunner = GetComponentInChildren<RaceRunner>(true);
            }
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            SendCustomEventDelayedFrames(nameof(_SetDriver), 1);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            _SetDriver();
        }
    }
}
