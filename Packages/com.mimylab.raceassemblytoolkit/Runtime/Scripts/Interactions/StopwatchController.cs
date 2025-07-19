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

    [Icon(ComponentIconPath.Stopwatch)]
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
        private bool _isHeld = false;
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

            RefreshDisplay();
        }

        private void Update()
        {
            InputDesktop();

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
            if (!_isHeld) { return; }
            if (!value) { return; }

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

        public override void OnPickup()
        {
            SendCustomEventDelayedFrames(nameof(_OnPostPickup), 1);
        }
        public void _OnPostPickup()
        {
            _isHeld = true;
        }

        public override void OnDrop()
        {
            _isHeld = false;
        }

        private int _selectedLap = 0;
        public void IncrementLap()
        {
            var lap = _stopwatch.Lap;
            if (lap < 1) { return; }

            _selectedLap = _selectedLap < lap ? _selectedLap + 1 : 1;

            if (_stopwatchDisplay)
            {
                _stopwatchDisplay.Lap = _selectedLap;
                _stopwatchDisplay.LapTime = _stopwatch.GetLapTime(_selectedLap);
                _stopwatchDisplay.SplitTime = _stopwatch.GetSplitTime(_selectedLap);
            }
        }

        public void DecrementLap()
        {
            var lap = _stopwatch.Lap;
            if (lap < 1) { return; }

            _selectedLap = _selectedLap > 1 ? _selectedLap - 1 : lap;

            if (_stopwatchDisplay)
            {
                _stopwatchDisplay.Lap = _selectedLap;
                _stopwatchDisplay.LapTime = _stopwatch.GetLapTime(_selectedLap);
                _stopwatchDisplay.SplitTime = _stopwatch.GetSplitTime(_selectedLap);
            }
        }

        private void InputDesktop()
        {
            if (_isUserInVR) { return; }
            if (!_isHeld) { return; }

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

            RefreshDisplay();
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

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            _selectedLap = _stopwatch.Lap;

            if (_stopwatchDisplay)
            {
                _stopwatchDisplay.MaxLaps = _selectedLap;
                _stopwatchDisplay.Lap = _selectedLap;
                _stopwatchDisplay.LapTime = _stopwatch.GetLapTime(_stopwatch.Lap);
                _stopwatchDisplay.SplitTime = _stopwatch.GetSplitTime(_stopwatch.Lap);
                _stopwatchDisplay.TotalTime = _isCounting ? _stopwatch.GetCurrentTime() : _stopwatch.GetTotalTime();
            }
        }
    }
}
