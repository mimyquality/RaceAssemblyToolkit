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

    public enum CheckpointReactionEmissionModule
    {
        NoChange = default,
        Enable,
        Disable
    }

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Interactions/CP Reaction Particle")]
    [RequireComponent(typeof(ParticleSystem))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CheckpointReactionParticle : ICheckpointReaction
    {
        [SerializeField]
        private CheckpointReactionEmissionModule _emissionModule = default;
        [SerializeField]
        private int _emit = 0;
        [SerializeField]
        private bool _driverOnly = false;

        private ParticleSystem _particle;

        private void Start()
        {
            _particle = GetComponent<ParticleSystem>();
        }

        public override void React(VRCPlayerApi driver)
        {
            if (_driverOnly && !driver.isLocal) { return; }

            if (_particle)
            {
                var emission = _particle.emission;
                switch (_emissionModule)
                {
                    case CheckpointReactionEmissionModule.Enable: emission.enabled = true; break;
                    case CheckpointReactionEmissionModule.Disable: emission.enabled = false; break;
                }

                if (_emit > 0)
                {
                    _particle.Emit(_emit);
                }
            }
        }
    }
}
