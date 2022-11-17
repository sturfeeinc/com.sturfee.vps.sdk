using NUnit.Framework;
using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XrSessionTest : MonoBehaviour
{
    [Test]
    public void CreateSession_NoProviders()
    {
        var location = new GeoLocation
        {
            Latitude = 37.332093,
            Longitude = -121.890137
        };

        XrSessionManager.CreateSession(location);
       
    }
}
