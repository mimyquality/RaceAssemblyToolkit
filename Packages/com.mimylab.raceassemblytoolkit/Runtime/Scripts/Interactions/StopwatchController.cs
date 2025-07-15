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
    using VRC.Udon.Common;
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Interactions/Stopwatch Controller")]
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StopwatchController : UdonSharpBehaviour
    {
        [SerializeField]
        private Stopwatch _stopwatch;
        [SerializeField]
        private StopwatchDisplay _stopwatchDisplay;

        private VRCPickup _pickup;
        private bool _isUserInVR = false;
        private bool _isCounting = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _pickup = GetComponent<VRCPickup>();
            _isUserInVR = Networking.LocalPlayer.IsUserInVR();

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_isUserInVR)
            {
                if (_pickup && _pickup.IsHeld)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            CountResetLap(Time.timeAsDouble);
                        }
                        else
                        {
                            CountStartStop(Time.timeAsDouble);
                        }
                    }
                }
            }

            if (_isCounting)
            {
                if (_stopwatchDisplay)
                {
                    _stopwatchDisplay.TotalTime = _stopwatch.GetCurrentTime();
                }
            }
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            var triggerClock = Time.timeAsDouble;

            if (!_isUserInVR) { return; }
            if (!value) { return; }
            if (!_pickup) { return; }
            if (!_pickup.IsHeld) { return; }

            if (args.handType == HandType.LEFT)
            {
                if (_pickup.currentHand == VRCPickup.PickupHand.Left)
                {
                    CountStartStop(triggerClock);
                }

                if (_pickup.currentHand == VRCPickup.PickupHand.Right)
                {
                    CountResetLap(triggerClock);
                }
            }

            if (args.handType == HandType.RIGHT)
            {
                if (_pickup.currentHand == VRCPickup.PickupHand.Right)
                {
                    CountStartStop(triggerClock);
                }

                if (_pickup.currentHand == VRCPickup.PickupHand.Left)
                {
                    CountResetLap(triggerClock);
                }
            }
        }

        private void CountStartStop(double triggerClock)
        {
            if (_isCounting)
            {
                _stopwatch.CountStop(triggerClock);
            }
            else
            {
                _stopwatch.CountStart(triggerClock);
            }

            _isCounting = _stopwatch.IsCounting;

            if (_stopwatchDisplay)
            {
                _stopwatchDisplay.MaxLaps = _stopwatch.MaxLaps;
                _stopwatchDisplay.Lap = _stopwatch.Lap;
                _stopwatchDisplay.LapTime = _stopwatch.GetLapTime(_stopwatch.Lap);
                _stopwatchDisplay.SplitTime = _stopwatch.GetSplitTime(_stopwatch.Lap);
                _stopwatchDisplay.TotalTime = _stopwatch.GetCurrentTime();
            }
        }

        private void CountResetLap(double triggerClock)
        {
            if (_isCounting)
            {
                _stopwatch.CountLap(triggerClock);
            }
            else
            {
                _stopwatch.CountReset();
            }

            _isCounting = _stopwatch.IsCounting;

            if (_stopwatchDisplay)
            {
                _stopwatchDisplay.MaxLaps = _stopwatch.MaxLaps;
                _stopwatchDisplay.Lap = _stopwatch.Lap;
                _stopwatchDisplay.LapTime = _stopwatch.GetLapTime(_stopwatch.Lap);
                _stopwatchDisplay.SplitTime = _stopwatch.GetSplitTime(_stopwatch.Lap);
                _stopwatchDisplay.TotalTime = _stopwatch.GetCurrentTime();
            }
        }
    }
}
