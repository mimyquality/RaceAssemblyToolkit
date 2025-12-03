/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/license/mit
*/

namespace MimyLab.RaceAssemblyToolkit
{
    //using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.RAT)]
    [AddComponentMenu("Race Assembly Toolkit/Core/Race Runner as Player")]
    [RequireComponent(typeof(VRCPlayerObject))]
    public class RaceRunnerAsPlayer : RaceRunner
    {

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            _variety = "Player";
        }
#endif

    }
}
