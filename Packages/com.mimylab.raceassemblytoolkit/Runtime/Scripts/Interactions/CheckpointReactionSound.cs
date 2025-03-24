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

    public enum CheckpointReactionAudioSourceState
    {
        NoChange = default,
        Play,
        Pause,
        Stop
    }

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Interactions/CP Reaction Sound")]
    [RequireComponent(typeof(AudioSource))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CheckpointReactionSound : ICheckpointReaction
    {
        [SerializeField]
        private CheckpointReactionAudioSourceState _audioSourceState = default;
        [SerializeField]
        private AudioClip _oneShotSound = null;
        [SerializeField]
        private bool _driverOnly = false;

        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public override void React(VRCPlayerApi driver)
        {
            if (_driverOnly && !driver.isLocal) { return; }

            if (_audioSource)
            {
                switch (_audioSourceState)
                {
                    case CheckpointReactionAudioSourceState.Play: _audioSource.Play(); break;
                    case CheckpointReactionAudioSourceState.Pause: _audioSource.Pause(); break;
                    case CheckpointReactionAudioSourceState.Stop: _audioSource.Stop(); break;
                }

                if (_oneShotSound)
                {
                    _audioSource.PlayOneShot(_oneShotSound);
                }
            }
        }
    }
}
