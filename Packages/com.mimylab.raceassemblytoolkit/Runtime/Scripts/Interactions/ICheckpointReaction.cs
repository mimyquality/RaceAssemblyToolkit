/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    using UdonSharp;
    //using UnityEngine;
    using VRC.SDKBase;

    abstract public class ICheckpointReaction : UdonSharpBehaviour
    {
        abstract public void React(VRCPlayerApi driver);
    }
}
