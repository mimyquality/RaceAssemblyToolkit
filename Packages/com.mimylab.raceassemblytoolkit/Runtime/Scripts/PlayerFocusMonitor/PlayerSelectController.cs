/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit.PlayerFocusMonitor
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    public enum PlayerSelectButtonType
    {
        Increment,
        Decrement
    }

    public enum PlayerSelectViewType
    {
        SelectedPlayerName,
    }

    [Icon(ComponentIconPath.PlayerFocusMonitor)]
    [AddComponentMenu("Race Assembly Toolkit/PlayerFocus Monitor/PlayerSelect Controller")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerSelectController : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerTracker _playerTracker;

        internal string selectedPlayerName = "";

        private VRCPlayerApi[] _players;
        private VRCPlayerApi _selectedPlayer;
        private VRCPlayerApi _localPlayer;

        private PlayerSelectButton[] _buttons = new PlayerSelectButton[0];
        private PlayerSelectView[] _views = new PlayerSelectView[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            var buttons = GetComponentsInChildren<PlayerSelectButton>();
            if (buttons != null)
            {
                _buttons = buttons;
                for (int i = 0; i < _buttons.Length; i++)
                {
                    _buttons[i].controller = this;
                }
            }

            var views = GetComponentsInChildren<PlayerSelectView>();
            if (views != null)
            {
                _views = views;
                for (int i = 0; i < _views.Length; i++)
                {
                    _views[i].controller = this;
                }
            }

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

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
            selectedPlayerName = player.displayName;

            if (_playerTracker)
            {
                _playerTracker.SetPlayer(player);
            }

            Feedback();
        }

        private void Feedback()
        {
            for (int i = 0; i < _views.Length; i++)
            {
                _views[i].UpdateView();
            }
        }
    }
}
