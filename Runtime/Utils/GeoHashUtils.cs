using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGeoHash;
namespace SturfeeVPS.SDK
{
    public class GeoHashUtils
    {
        private static readonly double[] GRID_WIDTHS = new double[] { 5009400.0, 1252300.0, 156500.0, 39100.0, 4900.0, 1200.0, 152.9, 38.2, 4.8, 1.2, 0.149, 0.0370 };
        private static readonly double[] GRID_HEIGHTS = new double[] { 4992600.0, 624100.0, 156000.0, 19500.0, 4900.0, 609.4, 152.4, 19.0, 4.8, 0.595, 0.149, 0.0199 };

        private class Vector2Doubles
        {
            public double x;
            public double y;
            public Vector2Doubles(double _x, double _y)
            {
                x = _x;
                y = _y;
            }
        }

        public static string[] EncodeWithRadius(double latitude, double longitude, double radius, int precision, int minlevel = 1, int maxlevel = 12)
        {
            return CreateGeohashList(latitude, longitude, radius, precision, minlevel, maxlevel);
        }

        private static bool IsInsideOfCircle(double latitude, double longitude, double centre_lat, double centre_lon, double radius)
        {
            double x_diff = longitude - centre_lon;
            double y_diff = latitude - centre_lat;

            if (Math.Pow(x_diff, 2) + Math.Pow(y_diff, 2) <= Math.Pow(radius, 2)) return true;
            return false;

        }

        private static Vector2Doubles GetCentroid(double latitude, double longitude, double height, double width)
        {
            double y_cen = latitude + (height / 2);
            double x_cen = longitude + (width / 2);
            return new Vector2Doubles(x_cen, y_cen);
        }

        private static Vector2Doubles PointToLatLon(double y, double x, double latitude, double longitude)
        {
            double pi = 3.14159265359;
            double r_earth = 6371000;

            double lat_diff = (y / r_earth) * (180 / pi);
            double lon_diff = (x / r_earth) * (180 / pi) / Math.Cos(latitude * pi / 180);

            double final_lat = latitude + lat_diff;
            double final_lon = longitude + lon_diff;

            return new Vector2Doubles(final_lat, final_lon);
        }

        private static string[] CreateGeohashList(double latitude, double longitude, double radius, int precision, int minlevel, int maxlevel)
        {
            if (radius == 0)
            {
                return new string[] { GeoHash.Encode(latitude, longitude, precision) };
            }

            double x = 0.0;
            double y = 0.0;

            List<Vector2Doubles> points = new List<Vector2Doubles>();

            double height = (GRID_HEIGHTS[precision - 1]) / 2;
            double width = (GRID_WIDTHS[precision - 1]) / 2;

            int lat_moves = (int)(Math.Ceiling(radius / height)); // 4
            int lon_moves = (int)(Math.Ceiling(radius / width)); // 2

            for (int i = 0; i < lat_moves; i++)
            {
                double temp_lat = y + height * i;
                for (int j = 0; j < lon_moves; j++)
                {
                    double temp_lon = x + width * j;
                    if (IsInsideOfCircle(temp_lat, temp_lon, y, x, radius))
                    {
                        var centroid = GetCentroid(temp_lat, temp_lon, height, width);
                        double x_cen = centroid.x;
                        double y_cen = centroid.y;

                        points.Add(PointToLatLon(y_cen, x_cen, latitude, longitude));
                        points.Add(PointToLatLon(-y_cen, x_cen, latitude, longitude));
                        points.Add(PointToLatLon(y_cen, -x_cen, latitude, longitude));
                        points.Add(PointToLatLon(-y_cen, -x_cen, latitude, longitude));
                    }
                }
            }

            List<string> geohashes = new List<string>();
            foreach (Vector2Doubles point in points)
            {
                geohashes.Add(GeoHash.Encode(point.x, point.y, precision));
            }
            geohashes = geohashes.Distinct().ToList();

            return geohashes.ToArray();
        }
    }
}