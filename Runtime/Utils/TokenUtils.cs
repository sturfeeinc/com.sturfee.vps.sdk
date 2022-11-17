using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public static class TokenUtils 
    {
        public static string GetVpsToken()
        {
            return SturfeeWindow.Auth.VpsToken;
            //return "sturfee_debug";
        }
    }
}
