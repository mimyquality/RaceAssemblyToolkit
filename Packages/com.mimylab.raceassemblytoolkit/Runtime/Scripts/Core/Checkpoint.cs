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
    [AddComponentMenu("Race Assembly Toolkit/Core/Checkpoint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Checkpoint : UdonSharpBehaviour
    {
        [SerializeField]
        private ICheckpointReaction[] _reactions = new ICheckpointReaction[0];

        internal CourseDescriptor course;
        internal RaceRunner[] entryRunners = new RaceRunner[0];
        internal RaceRunnerAsPlayer entryRunnerAsPlayer;
        internal RaceRunnerAsDrone entryRunnerAsDrone;

        private void OnTriggerEnter(Collider other)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(other)) { return; }

            var runner = other.GetComponent<RaceRunner>();
            if (!runner) { return; }
            if (System.Array.IndexOf(entryRunners, runner) < 0) { return; }

            var driver = runner.GetDriver();
            if (!Utilities.IsValid(driver)) { return; }

            if (driver.isLocal)
            {
                runner.OnCheckpointPassed(this, triggerClock);
            }

            React(driver);
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(player)) { return; }
            if (!Utilities.IsValid(entryRunnerAsPlayer)) { return; }

            var runnerAsPlayer = (RaceRunner)player.FindComponentInPlayerObjects(entryRunnerAsPlayer);
            if (!runnerAsPlayer) { return; }

            if (player.isLocal)
            {
                runnerAsPlayer.OnCheckpointPassed(this, triggerClock);
            }

            React(player);
        }

        //public override void OnDroneTriggerEnter(VRCDroneApi drone) { }

        private void React(VRCPlayerApi driver)
        {
            for (int i = 0; i < _reactions.Length; i++)
            {
                if (_reactions[i])
                {
                    _reactions[i].React(driver);
                }
            }
        }
    }
}
