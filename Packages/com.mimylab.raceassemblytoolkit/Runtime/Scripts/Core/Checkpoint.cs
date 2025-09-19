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

        private RaceRunner[] participateRunners = new RaceRunner[0];
        private RaceRunnerAsPlayer participateRunnerAsPlayer;
        private RaceRunnerAsDrone participateRunnerAsDrone;

        private void OnTriggerEnter(Collider other)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(other)) { return; }

            var runner = other.GetComponent<RaceRunner>();
            if (!runner) { return; }
            if (System.Array.IndexOf(participateRunners, runner) < 0) { return; }

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

            if (!Utilities.IsValid(participateRunnerAsPlayer)) { return; }
            if (!Utilities.IsValid(player)) { return; }

            var runnerAsPlayer = (RaceRunner)player.FindComponentInPlayerObjects(participateRunnerAsPlayer);
            if (!runnerAsPlayer) { return; }

            if (player.isLocal)
            {
                runnerAsPlayer.OnCheckpointPassed(this, triggerClock);
            }

            React(player);
        }

        public override void OnDroneTriggerEnter(VRCDroneApi drone)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(participateRunnerAsDrone)) { return; }
            if (!Utilities.IsValid(drone)) { return; }

            var driver = drone.GetPlayer();
            if (!Utilities.IsValid(driver)) { return; }

            var runnerAsDrone = (RaceRunner)driver.FindComponentInPlayerObjects(participateRunnerAsDrone);
            if (!runnerAsDrone) { return; }

            if (driver.isLocal)
            {
                runnerAsDrone.OnCheckpointPassed(this, triggerClock);
            }

            React(driver);
        }

        internal void SetCourseSettings(CourseDescriptor course)
        {
            this.course = course;
            participateRunners = course.runners;
            participateRunnerAsPlayer = course.runnerAsPlayer;
            participateRunnerAsDrone = course.runnerAsDrone;
        }

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
