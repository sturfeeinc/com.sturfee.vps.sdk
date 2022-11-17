using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SturfeeWindow
    {
        public static SturfeeWindowConfig Config => SturfeeWindowConfig.Config;
        
        public static SturfeeWindowAuth Auth => SturfeeWindowAuth.Auth;
        
    }
}
