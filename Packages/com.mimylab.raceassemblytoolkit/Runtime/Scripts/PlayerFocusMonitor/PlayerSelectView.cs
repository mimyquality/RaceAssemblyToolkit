/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit.PlayerFocusMonitor
{
    using TMPro;
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    [Icon(ComponentIconPath.PlayerFocusMonitor)]
    [AddComponentMenu("Race Assembly Toolkit/PlayerFocus Monitor/PlayerSelect View")]
    [RequireComponent(typeof(TMP_Text))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerSelectView : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerSelectViewType _viewType;

        internal PlayerSelectController controller;

        private TMP_Text _text;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _text = GetComponent<TMP_Text>();

            _initialized = true;
        }

        public void UpdateView()
        {
            Initialize();

            if (!controller) { return; }            

            switch (_viewType)
            {
                case PlayerSelectViewType.SelectedPlayerName:
                    _text.text = controller.selectedPlayerName;
                    break;
            }
        }
    }
}
