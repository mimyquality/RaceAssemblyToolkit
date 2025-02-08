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

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Checkpoint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Checkpoint : UdonSharpBehaviour
    {
        internal CourseDescriptor course;

        private void OnTriggerEnter(Collider other)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(other)) { return; }

            var runner = other.GetComponent<Runner>();
            if (!runner) { return; }

            runner.OnPassCheckpoint(this, triggerClock);
        }
    }
}
