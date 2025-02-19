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

            var runner = other.GetComponent<RaceRunner>();
            if (!runner) { return; }

            // ToDo:チェックポイント通過イベント

            var driver = runner.GetDriver();
            if (Utilities.IsValid(driver) && driver.isLocal)
            {
                runner.OnCheckpointPassed(this, triggerClock);
            }
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(player)) { return; }

            var playerObjects = player.GetPlayerObjects();
            for (int i = 0; i < playerObjects.Length; i++)
            {
                var driverAsPlayer = playerObjects[i].GetComponent<RaceDriverAsPlayer>();
                if (!driverAsPlayer) { continue; }

                var runner = driverAsPlayer.targetRunner;
                if (!runner) { return; }

                // ToDo:チェックポイント通過イベント

                if (player.isLocal)
                {
                    runner.OnCheckpointPassed(this, triggerClock);
                }

                return;
            }
        }
    }
}
