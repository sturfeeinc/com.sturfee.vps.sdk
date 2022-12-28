using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class ScanConfig
    {
        public float YawAngle;
        public int TargetCount;
        public int PitchMin;
        public int PitchMax;
        public int RollMin;
        public int RollMax;
        public ScanType ScanType;
        public OffsetType OffsetType;
    }
}
