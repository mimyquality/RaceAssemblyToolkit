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

    public class RaceDriver : UdonSharpBehaviour
    {
        public RaceRunner targetRunner;

        public void _SetDriver()
        {
            if (targetRunner)
            {
                targetRunner.SetDriver(Networking.GetOwner(this.gameObject));
            }
        }
    }
}