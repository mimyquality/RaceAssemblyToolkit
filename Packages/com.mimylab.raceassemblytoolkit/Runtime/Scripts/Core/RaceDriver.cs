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
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Driver")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RaceDriver : UdonSharpBehaviour
    {
        internal RaceRunner raceRunner;

        private void OnEnable()
        {
            _SetDriver();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            _SetDriver();
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            _SetDriver();
        }

        public void _SetDriver()
        {
            if (raceRunner)
            {
                raceRunner.SetDriver(Networking.GetOwner(this.gameObject));
            }
        }
    }
}
