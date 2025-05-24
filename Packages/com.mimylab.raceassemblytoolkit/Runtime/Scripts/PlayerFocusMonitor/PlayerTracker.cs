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

    public enum PlayerTrackerFocusPoint
    {
        Origin,
        EyeHeight,
        Head,
        rightHand,
        leftHand,
    }

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Player Focus Monitor/Player Tracker")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerTracker : UdonSharpBehaviour
    {
        public PlayerTrackerFocusPoint focusPoint = PlayerTrackerFocusPoint.EyeHeight;

        private VRCPlayerApi _targetPlayer;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }



            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(_targetPlayer)) { return; }

            var rot = _targetPlayer.GetRotation();
            var pos = Vector3.zero;
            switch (focusPoint)
            {
                //case PlayerTrackerFocusPoint.Origin:
                //    break;
                case PlayerTrackerFocusPoint.EyeHeight:
                    pos = _targetPlayer.GetPosition();
                    pos.y += _targetPlayer.GetAvatarEyeHeightAsMeters();
                    break;
                case PlayerTrackerFocusPoint.Head:
                    pos = _targetPlayer.GetBonePosition(HumanBodyBones.Head);
                    break;
                case PlayerTrackerFocusPoint.rightHand:
                    pos = _targetPlayer.GetBonePosition(HumanBodyBones.RightHand);
                    break;
                case PlayerTrackerFocusPoint.leftHand:
                    pos = _targetPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                    break;
            }
            this.transform.SetPositionAndRotation(pos, rot);
        }

        public void SetPlayer(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) { return; }

            _targetPlayer = player;
        }

        public void SetPlayerById(int playerId)
        {
            SetPlayer(VRCPlayerApi.GetPlayerById(playerId));
        }
    }
}
