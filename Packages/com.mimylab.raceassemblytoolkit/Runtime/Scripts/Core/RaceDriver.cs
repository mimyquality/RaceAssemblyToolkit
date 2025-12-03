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
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Driver")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RaceDriver : UdonSharpBehaviour
    {
        [SerializeField]
        private RaceRunner _raceRunner;

        private void OnEnable()
        {
            SetDriver(Networking.GetOwner(this.gameObject));
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            SetDriver(player);
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            if (newMaster.IsOwner(this.gameObject))
            {
                SetDriver(newMaster);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var runner = (RaceRunner)player.FindComponentInPlayerObjects(_raceRunner);
            runner.SetDriver(Networking.GetOwner(this.gameObject));
        }

        private void SetDriver(VRCPlayerApi driver)
        {
            if (!_raceRunner) { return; }

            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
            for (int i = 0; i < players.Length; i++)
            {
                if (!Utilities.IsValid(players[i])) { continue; }

                var runner = (RaceRunner)players[i].FindComponentInPlayerObjects(_raceRunner);
                if (!Utilities.IsValid(runner)) { continue; }

                runner.SetDriver(driver);
            }
        }
    }
}
