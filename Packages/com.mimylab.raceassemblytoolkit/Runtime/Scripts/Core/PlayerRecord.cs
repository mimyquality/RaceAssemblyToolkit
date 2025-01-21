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
    using VRC.Udon;
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Player Record")]
    [RequireComponent(typeof(VRCPlayerObject), typeof(VRCEnablePersistence))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerRecord : UdonSharpBehaviour
    {
        internal CourseDescriptor course;

        [UdonSynced]
        private float _bestRecordTime;
        [UdonSynced]
        private string _bestRecordVehicle = "";

        [UdonSynced]
        private float _recordTime;
        [UdonSynced]
        private string _recordVehicle = "";

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }



            _initialized = true;
        }
        private void Start()
        {
            Initialize();


        }
    }
}
