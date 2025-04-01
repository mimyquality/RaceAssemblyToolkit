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
        public RaceRunner targetRunner;

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

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            _SetDriver();
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            SendCustomEventDelayedFrames(nameof(_SetDriver), 1);
        }
        
        public void _SetDriver()
        {
            if (targetRunner)
            {
                targetRunner.SetDriver(Networking.GetOwner(this.gameObject));
            }
        }
    }
}
