using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGeoHash;
namespace SturfeeVPS.SDK
{
    public class Vector2Doubles
    {
        public double x;
        public double y;
        public Vector2Doubles(double _x, double _y)
        {
            x = _x;
            y = _y;
        }
    }

    public static class GeoHashExtension
    {
        public static bool in_circle_check(double latitude, double longitude, double centre_lat, double centre_lon, double radius)
        {
            double x_diff = longitude - centre_lon;
            double y_diff = latitude - centre_lat;

            if ( Math.Pow(x_diff,2) + Math.Pow(y_diff,2) <= Math.Pow(radius,2) ) return true;
            return false;
            
        }

        public static Vector2Doubles get_centroid(double latitude, double longitude, double height, double width)
        {
            double y_cen = latitude + (height/2);
            double x_cen = longitude + (width/2);
            return new Vector2Doubles(x_cen, y_cen);
        }

        public static Vector2Doubles convert_to_latlon(double y, double x, double latitude, double longitude)
        {
            double pi = 3.14159265359;
            double r_earth = 6371000;

            double lat_diff = (y / r_earth) * (180 / pi);
            double lon_diff = (x / r_earth) * (180 / pi) / Math.Cos(latitude * pi/180);

            double final_lat = latitude + lat_diff;
            double final_lon = longitude + lon_diff;

            return new Vector2Doubles(final_lat, final_lon);
        }

        public static string[] create_geohash(double latitude, double longitude, double radius, int precision, bool georaptor_flag=false, int minlevel=1, int maxlevel=12)
        {
            double x = 0.0;
            double y = 0.0;
            
            List<Vector2Doubles> points = new List<Vector2Doubles>();
            double[] grid_width = new double[]{5009400.0, 1252300.0, 156500.0, 39100.0, 4900.0, 1200.0, 152.9, 38.2, 4.8, 1.2, 0.149, 0.0370};
            double[] grid_height = new double[]{4992600.0, 624100.0, 156000.0, 19500.0, 4900.0, 609.4, 152.4, 19.0, 4.8, 0.595, 0.149, 0.0199};

            double height = (grid_height[precision - 1])/2;
            double width = (grid_width[precision-1])/2 ;

            int lat_moves = (int) (Math.Ceiling(radius / height)); // 4
            int lon_moves = (int) (Math.Ceiling(radius / width)); // 2

            for (int i=0; i<lat_moves; i++)
            {
                double temp_lat = y + height*i;
                for (int j=0; j<lon_moves; j++)
                {
                    double temp_lon = x + width*j;
                    if (in_circle_check(temp_lat, temp_lon, y, x, radius))
                    {
                        var centroid = get_centroid(temp_lat, temp_lon, height, width);
                        double x_cen = centroid.x;
                        double y_cen = centroid.y;

                        points.Add(convert_to_latlon(y_cen, x_cen, latitude, longitude));
                        points.Add(convert_to_latlon(-y_cen, x_cen, latitude, longitude));
                        points.Add(convert_to_latlon(y_cen, -x_cen, latitude, longitude));
                        points.Add(convert_to_latlon(-y_cen, -x_cen, latitude, longitude));
                    }
                }
            }
            
            List<string> geohashes = new List<string>();
            foreach(Vector2Doubles point in points)
            {
                geohashes.Add(GeoHash.Encode(point.x, point.y, precision));
            }
            geohashes = geohashes.Distinct().ToList();

            return geohashes.ToArray();
        }
    }
}