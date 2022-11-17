using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SturfeeVPS.SDK
{
    public static class VpsServices 
    {
        public async static Task CheckCoverage(GeoLocation location, string token)
        {
            SturfeeDebug.Log(" Checking for localization coverage");

            UnityWebRequest unityWebRequest = UnityWebRequest.Get($"{SturfeeConstants.STURFEE_API}/alignment_available/?lat={location.Latitude}&lng={location.Longitude}&token={token}");
            unityWebRequest.timeout = 3;
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + token);
            unityWebRequest.SetRequestHeader("latitude", location.Latitude.ToString());
            unityWebRequest.SetRequestHeader("longitude", location.Longitude.ToString());

            await unityWebRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(unityWebRequest.error))
            {
                throw new HttpException(unityWebRequest.responseCode, unityWebRequest.error);
            }

            SturfeeDebug.Log("Localization available at this location");
        }

        public static async Task ValidateToken(string token)
        {
            SturfeeDebug.Log(" Validating token...");

            UnityWebRequest unityWebRequest = UnityWebRequest.Get($"{SturfeeConstants.STURFEE_API}/status/?accessToken={token}");
            unityWebRequest.timeout = 3;
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + token);

            await unityWebRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(unityWebRequest.error))
            {
                throw new HttpException(unityWebRequest.responseCode, unityWebRequest.error);
            }

            SturfeeDebug.Log("Token check complete. Token is valid !");
        }
    }
}
