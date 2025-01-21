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
    using VRC.Udon;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Time Counter")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TimeCounter : UdonSharpBehaviour
    {

        internal PlayerRecord playerRecord;

        private CourseDescriptor _entryCourse;
        private Checkpoint _lastPassedCheckpoint;

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

        private void OnTriggerEnter(Collider other)
        {
            var triggerTime = Time.time;

            if (!Utilities.IsValid(other)) { return; }

            var checkpoint = other.GetComponent<Checkpoint>();
            if (!checkpoint) { return; }

        }

        private bool ValidateCheckpoint(Checkpoint checkpoint)
        {
            var course = checkpoint.course;
            if (_entryCourse && course != _entryCourse) { return false; }

            // ToDo:コース設定
            //if(course.lapMode == CourseLapMode.OnePass)


            // ToDo:コース設定
            //if(course.lapMode == CourseLapMode.Circuit)

            return false;
        }

        private void CountStart()
        {

        }

        private void CountLap()
        {

        }

        private void CountStop()
        {

        }
    }
}
