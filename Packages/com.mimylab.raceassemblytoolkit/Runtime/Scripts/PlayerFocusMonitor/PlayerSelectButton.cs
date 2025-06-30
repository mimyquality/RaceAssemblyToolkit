/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit.PlayerFocusMonitor
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.UI;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/PlayerFocus Monitor/PlayerSelect Button")]
    [RequireComponent(typeof(Button))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerSelectButton : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerSelectButtonType _switchType;

        internal PlayerSelectController controller;

        private Button _button;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _button = GetComponent<Button>();

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            _button.interactable = true;
        }

        private void OnDisable()
        {
            _button.interactable = false;
        }

        public override void Interact()
        {
            if (!controller) { return; }

            switch (_switchType)
            {
                case PlayerSelectButtonType.Increment: controller.Increment(); break;
                case PlayerSelectButtonType.Decrement: controller.Decrement(); break;
            }
        }
    }
}
