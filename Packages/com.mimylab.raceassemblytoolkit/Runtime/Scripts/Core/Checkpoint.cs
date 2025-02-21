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
        [SerializeField, Header("Activate/Inactivate GameObject")]
        private GameObject _activateObject;
        [SerializeField]
        private GameObject _inactivateObject;
        [SerializeField, Min(0), Tooltip("Reactivate after specified time if Duration > 0")]
        private float _duration = 0.0f;

        [Header("Emit particle")]
        [SerializeField]
        private ParticleSystem _particleSystem;
        [SerializeField, Min(0)]
        private int _emit = 0;

        [Header("Play sound one shot")]
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private AudioClip _audioClip;

        [Header("Execute SendCustomEvent to other UdonBehaviour")]
        [SerializeField,]
        private UdonBehaviour _udonBehaviour;
        [SerializeField]
        private string _eventName = "";

        internal CourseDescriptor course;

        private int _triggerCount = 0;

        private void Start()
        {
            if (!_particleSystem) { _particleSystem = GetComponent<ParticleSystem>(); }
            if (!_audioSource) { _audioSource = GetComponent<AudioSource>(); }
            if (_audioSource) { if (!_audioClip) { _audioClip = _audioSource.clip; } }
        }

        private void OnTriggerEnter(Collider other)
        {
            var triggerClock = Time.timeAsDouble;

            if (!Utilities.IsValid(other)) { return; }

            var runner = other.GetComponent<RaceRunner>();
            if (!runner) { return; }

            ReactiveRunnerTrigger();

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

                ReactiveRunnerTrigger();

                if (player.isLocal)
                {
                    runner.OnCheckpointPassed(this, triggerClock);
                }

                return;
            }
        }

        public void _ReactivateGameObject()
        {
            if (--_triggerCount > 0) { return; }

            if (_activateObject) { _activateObject.SetActive(false); }
            if (_inactivateObject) { _inactivateObject.SetActive(true); }
        }

        private void ReactiveRunnerTrigger()
        {
            if (_activateObject || _inactivateObject)
            {
                if (_activateObject) { _activateObject.SetActive(true); }
                if (_inactivateObject) { _inactivateObject.SetActive(false); }

                if (_duration > 0.0f)
                {
                    _triggerCount++;
                    SendCustomEventDelayedSeconds(nameof(_ReactivateGameObject), _duration);
                }
            }
            if (_particleSystem && _emit > 0) { _particleSystem.Emit(_emit); }
            if (_audioSource && _audioClip) { _audioSource.PlayOneShot(_audioClip); }
            if (_udonBehaviour && _eventName != "") { _udonBehaviour.SendCustomEvent(_eventName); }
        }
    }
}
