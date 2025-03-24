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
    [AddComponentMenu("Race Assembly Toolkit/Interactions/CP Reaction Object")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CheckpointReactionObject : ICheckpointReaction
    {
        [SerializeField]
        private bool _driverOnly = false;
        [SerializeField]
        private GameObject[] _activateObjects = new GameObject[0];
        [SerializeField]
        private GameObject[] _deactivateObjects = new GameObject[0];


        public override void React(VRCPlayerApi driver)
        {
            if (_driverOnly && !driver.isLocal) { return; }

            for (int i = 0; i < _activateObjects.Length; i++)
            {
                if (_activateObjects[i])
                {
                    _activateObjects[i].SetActive(true);
                }
            }

            for (int i = 0; i < _deactivateObjects.Length; i++)
            {
                if (_deactivateObjects[i])
                {
                    _deactivateObjects[i].SetActive(false);
                }
            }
        }
    }
}
