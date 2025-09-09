/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;

    public abstract class IRaceEventReceiver : UdonSharpBehaviour
    {
        abstract internal void AddRunner(RaceRunner runner);
        abstract internal void AddRunners(RaceRunner[] runners);
        abstract internal void RemoveRunner(RaceRunner runner);
        abstract internal void ClearRunners();
        abstract internal void OnRunnerUpdated(RaceRunner runner);
    }
}