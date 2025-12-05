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

        private RaceRunnerAsPlayer _runnerAsPlayer;
        private RaceRunnerAsDrone _runnerAsDrone;

        private void OnTriggerEnter(Collider other)
        {
            var triggerClock = Time.timeAsDouble;

            if (!course) { return; }
            if (!Utilities.IsValid(other)) { return; }

            var runner = other.GetComponent<RaceRunner>();
            if (!runner) { return; }
            if (!course.IsParticipatingRunner(runner)) { return; }

            if (Networking.IsOwner(runner.gameObject))
            {
                runner.OnCheckpointPassed(this, triggerClock);
            }

            React(runner.GetDriver());
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            var triggerClock = Time.timeAsDouble;

            if (!_runnerAsPlayer) { return; }
            if (!Utilities.IsValid(player)) { return; }

            var runner = (RaceRunner)player.FindComponentInPlayerObjects(_runnerAsPlayer);
            if (!runner) { return; }

            if (player.isLocal)
            {
                runner.OnCheckpointPassed(this, triggerClock);
            }

            React(player);
        }

        public override void OnDroneTriggerEnter(VRCDroneApi drone)
        {
            var triggerClock = Time.timeAsDouble;

            if (!_runnerAsDrone) { return; }
            if (!Utilities.IsValid(drone)) { return; }

            var player = drone.GetPlayer();
            if (!Utilities.IsValid(player)) { return; }

            var runner = (RaceRunner)player.FindComponentInPlayerObjects(_runnerAsDrone);
            if (!runner) { return; }

            if (player.isLocal)
            {
                runner.OnCheckpointPassed(this, triggerClock);
            }

            React(player);
        }

        internal void SetCourse(CourseDescriptor course)
        {
            this.course = course;
            _runnerAsPlayer = course.runnerAsPlayer;
            _runnerAsDrone = course.runnerAsDrone;
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
