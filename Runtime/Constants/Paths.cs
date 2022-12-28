using System;
using System.IO;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public static class Paths
    {
        public static readonly string SturfeeResourcesRelative = Path.Combine("Sturfee", "SturfeeConfiguration");
        public static readonly string SturfeeResourcesAbsolute = Path.Combine(Path.Combine(Application.dataPath, "Resources"), "Sturfee");

        public static readonly string ConfigFile = "SturfeeConfiguration.txt";

    }
}