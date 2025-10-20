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

    public abstract class IRecordReceiver : UdonSharpBehaviour
    {
        abstract internal void OnRaceRecordUpdate(RaceRecord raceRecord);
    }
}