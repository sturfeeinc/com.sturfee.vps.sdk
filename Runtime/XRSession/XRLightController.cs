using System;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    //https://gist.github.com/paulhayes/54a7aa2ee3cccad4d37bb65977eb19e2
    [RequireComponent(typeof(Light))]
    public class XRLightController : MonoBehaviour
    {
        private const double DEG2RAD = Math.PI / 180.0;
        private const double RAD2DEG = 180.0 / Math.PI;

        private Quaternion _direction = Quaternion.identity;
        private float _intensity;

        private double _longitude;
        private double _latitude;

        private DateTime _time;

        private Light _light;

        private void Awake()
        {
            SturfeeEventManager.Instance.OnSessionReady += OnSessionReady;

            ManageLayers();
        }

        private void Start()
        {
            _light = GetComponent<Light>();
        }

        private void OnSessionReady()
        {
            _latitude = XRSessionManager.GetSession().GetXRCameraLocation().Latitude;
            _longitude = XRSessionManager.GetSession().GetXRCameraLocation().Longitude;
            _time = DateTime.Now;

            SetPosition();

            _light.intensity = _intensity;
            _light.transform.rotation = _direction;
        }

        private void OnDestroy()
        {
            SturfeeEventManager.Instance.OnSessionReady -= OnSessionReady;
        }

        private void SetPosition()
        {
            Vector3 angles = new Vector3();
            double alt;
            double azi;
            CalculateSunPosition(_time, _latitude, _longitude, out azi, out alt);
            angles.x = (float)alt * Mathf.Rad2Deg;
            angles.y = (float)azi * Mathf.Rad2Deg;

            // Fix
            angles.y -= 180;

            _direction = Quaternion.Euler(angles);

            //_intensity = Mathf.InverseLerp(-12, 0, angles.x);
            _intensity = 1; // must be 1 for our building hide shader; change shadow instead
        }



        /*! 
         * \brief Calculates the sun light. 
         * 
         * CalcSunPosition calculates the suns "position" based on a 
         * given date and time in local time, latitude and longitude 
         * expressed in decimal degrees. It is based on the method 
         * found here: 
         * http://www.astro.uio.no/~bgranslo/aares/calculate.html 
         * The calculation is only satisfiably correct for dates in 
         * the range March 1 1900 to February 28 2100. 
         * \param dateTime Time and date in local time. 
         * \param latitude Latitude expressed in decimal degrees. 
         * \param longitude Longitude expressed in decimal degrees. 
         */
        private void CalculateSunPosition(DateTime dateTime, double latitude, double longitude, out double outAzimuth, out double outAltitude)
        {
            // Convert to UTC  
            dateTime = dateTime.ToUniversalTime();

            // Number of days from J2000.0.  
            double julianDate = 367 * dateTime.Year -
                (int)((7.0 / 4.0) * (dateTime.Year +
                    (int)((dateTime.Month + 9.0) / 12.0))) +
                (int)((275.0 * dateTime.Month) / 9.0) +
                dateTime.Day - 730531.5;

            double julianCenturies = julianDate / 36525.0;

            // Sidereal Time  
            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

            double siderealTimeUT = siderealTimeHours +
                (366.2422 / 365.2422) * (double)dateTime.TimeOfDay.TotalHours;

            double siderealTime = siderealTimeUT * 15 + longitude;

            // Refine to number of days (fractional) to specific time.  
            julianDate += (double)dateTime.TimeOfDay.TotalHours / 24.0;
            julianCenturies = julianDate / 36525.0;

            // Solar Coordinates  
            double meanLongitude = CorrectAngle(DEG2RAD *
                (280.466 + 36000.77 * julianCenturies));

            double meanAnomaly = CorrectAngle(DEG2RAD *
                (357.529 + 35999.05 * julianCenturies));

            double equationOfCenter = DEG2RAD * ((1.915 - 0.005 * julianCenturies) *
                Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

            double elipticalLongitude =
                CorrectAngle(meanLongitude + equationOfCenter);

            double obliquity = (23.439 - 0.013 * julianCenturies) * DEG2RAD;

            // Right Ascension  
            double rightAscension = Math.Atan2(
                Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
                Math.Cos(elipticalLongitude));

            double declination = Math.Asin(
                Math.Sin(rightAscension) * Math.Sin(obliquity));

            // Horizontal Coordinates  
            double hourAngle = CorrectAngle(siderealTime * DEG2RAD) - rightAscension;

            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }

            double altitude = Math.Asin(Math.Sin(latitude * DEG2RAD) *
                Math.Sin(declination) + Math.Cos(latitude * DEG2RAD) *
                Math.Cos(declination) * Math.Cos(hourAngle));

            // Nominator and denominator for calculating Azimuth  
            // angle. Needed to test which quadrant the angle is in.  
            double aziNom = -Math.Sin(hourAngle);
            double aziDenom =
                Math.Tan(declination) * Math.Cos(latitude * DEG2RAD) -
                Math.Sin(latitude * DEG2RAD) * Math.Cos(hourAngle);

            double azimuth = Math.Atan(aziNom / aziDenom);

            if (aziDenom < 0) // In 2nd or 3rd quadrant  
            {
                azimuth += Math.PI;
            }
            else if (aziNom < 0) // In 4th quadrant  
            {
                azimuth += 2 * Math.PI;
            }

            outAltitude = altitude;
            outAzimuth = azimuth;
        }

        /*! 
    * \brief Corrects an angle. 
    * 
    * \param angleInRadians An angle expressed in radians. 
    * \return An angle in the range 0 to 2*PI. 
    */
        private double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians % (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }

        private void ManageLayers()
        {
            Light light = GetComponent<Light>();
            int layer;

            // Building
            layer = LayerMask.NameToLayer(SturfeeLayers.Building);
            if ((light.cullingMask & (1 << layer)) == 0)
            {
                light.cullingMask |= 1 << LayerMask.NameToLayer(SturfeeLayers.Building);
            }

            // Terrain
            layer = LayerMask.NameToLayer(SturfeeLayers.Terrain);
            if ((light.cullingMask & (1 << layer)) == 0)
            {
                light.cullingMask |= 1 << LayerMask.NameToLayer(SturfeeLayers.Terrain);
            }


            // Background (Remove)
            layer = LayerMask.NameToLayer(SturfeeLayers.Background);
            if ((light.cullingMask & (1 << layer)) != 0)
            {
                light.cullingMask &= ~(1 << LayerMask.NameToLayer(SturfeeLayers.Background));
            }
        }
    }
}