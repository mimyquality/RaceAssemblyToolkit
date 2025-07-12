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

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!targetRunner) { targetRunner = GetComponentInChildren<RaceRunner>(true); }

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            _SetDriver();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            Initialize();

            _SetDriver();
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            Initialize();

            _SetDriver();
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
