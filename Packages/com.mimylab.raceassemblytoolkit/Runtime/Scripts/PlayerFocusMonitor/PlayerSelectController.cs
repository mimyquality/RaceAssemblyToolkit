/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using TMPro;
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/PlayerFocus Monitor/PlayerSelect Controller")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerSelectController : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerTracker _playerTracker;
        [SerializeField]
        private TMP_Text _playerNameText;

        private VRCPlayerApi[] _players;
        private VRCPlayerApi _selectedPlayer;
        private VRCPlayerApi _localPlayer;

        private void OnEnable()
        {
            _localPlayer = Networking.LocalPlayer;

            _RefreshPlayers();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayers), 1);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayers), 1);
        }

        public void Select(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) { return; }
            if (System.Array.IndexOf(_players, player) < 0) { return; }

            SelectPlayer(player);
        }

        public void Increment()
        {
            var nextPlayerIndex = System.Array.IndexOf(_players, _selectedPlayer) + 1;
            if (nextPlayerIndex < 1)
            {
                nextPlayerIndex = System.Array.IndexOf(_players, _localPlayer);
            }
            if (nextPlayerIndex >= _players.Length)
            {
                nextPlayerIndex = 0;
            }

            SelectPlayer(Utilities.IsValid(_players[nextPlayerIndex]) ? _players[nextPlayerIndex] : _localPlayer);
        }

        public void Decrement()
        {
            var nextPlayerIndex = System.Array.IndexOf(_players, _selectedPlayer) - 1;
            if (nextPlayerIndex < -1)
            {
                nextPlayerIndex = System.Array.IndexOf(_players, _localPlayer);
            }
            if (nextPlayerIndex < 0)
            {
                nextPlayerIndex = _players.Length - 1;
            }

            SelectPlayer(Utilities.IsValid(_players[nextPlayerIndex]) ? _players[nextPlayerIndex] : _localPlayer);
        }

        public void _RefreshPlayers()
        {
            _players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);

            SelectPlayer(Utilities.IsValid(_selectedPlayer) ? _selectedPlayer : _localPlayer);
        }

        private void SelectPlayer(VRCPlayerApi player)
        {
            _selectedPlayer = player;

            if (_playerTracker)
            {
                _playerTracker.SetPlayer(player);
            }
            
            if (_playerNameText)
            {
                _playerNameText.text = player.displayName;
            }
        }
    }
}
