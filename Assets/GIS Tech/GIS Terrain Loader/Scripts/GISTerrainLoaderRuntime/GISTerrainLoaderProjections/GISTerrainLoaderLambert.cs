/*     Unity GIS Tech 2020-2021      */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public enum LambertZone
    {
        LambertI = 0, LambertII = 1, LambertIII = 2, LambertIV = 3, LambertIIExtended = 4, Lambert93 = 5
    }

    public class GISTerrainLoaderLambert
    {
        private static double latitudeFromLatitudeISO(double latISo, double e, double eps)
        {

            double phi0 = 2 * Math.Atan(Math.Exp(latISo)) - LambertZoneCal.M_PI_2;
            double phiI = 2 * Math.Atan(Math.Pow((1 + e * Math.Sin(phi0)) / (1 - e * Math.Sin(phi0)), e / 2d) * Math.Exp(latISo)) - LambertZoneCal.M_PI_2;
            double delta = Math.Abs(phiI - phi0);

            while (delta > eps)
            {
                phi0 = phiI;
                phiI = 2 * Math.Atan(Math.Pow((1 + e * Math.Sin(phi0)) / (1 - e * Math.Sin(phi0)), e / 2d) * Math.Exp(latISo)) - LambertZoneCal.M_PI_2;
                delta = Math.Abs(phiI - phi0);
            }

            return phiI;
        }

        private static DVector3 lambertToGeographic(DVector3 org, LambertZoneCal zone, double lonMeridian, double e, double eps)
        {
            double n = zone.n();
            double C = zone.c();
            double xs = zone.xs();
            double ys = zone.ys();

            double x = org.x;
            double y = org.y;


            double lon, gamma, R, latIso;

            R = Math.Sqrt((x - xs) * (x - xs) + (y - ys) * (y - ys));

            gamma = Math.Atan((x - xs) / (ys - y));

            lon = lonMeridian + gamma / n;

            latIso = -1 / n * Math.Log(Math.Abs(R / C));

            double lat = latitudeFromLatitudeISO(latIso, e, eps);

            DVector3 dest = new DVector3(lon, lat, 0);
            return dest;
        }
        private static DVector3 GeographicToLambert(DVector2 LatLon, LambertZoneCal zone, double lonMeridian, double e)
        {

            double n = zone.n();
            double C = zone.c();
            double xs = zone.xs();
            double ys = zone.ys();

            double sinLat = Math.Sin(LatLon.y);
            double eSinLat = (e * sinLat);
            double elt1 = (1 + sinLat) / (1 - sinLat);
            double elt2 = (1 + eSinLat) / (1 - eSinLat);

            double latIso = (1 / 2d) * Math.Log(elt1) - (e / 2d) * Math.Log(elt2);

            double R = C * Math.Exp(-(n * latIso));

            double LAMBDA = n * (LatLon.x - lonMeridian);

            double x = xs + (R * Math.Sin(LAMBDA));
            double y = ys - (R * Math.Cos(LAMBDA));

            return new DVector3(x, y, 0);
        }
        public static DVector3 LatLonToLambert(DVector2 LatLon, LambertZone zone)
        {
            var lzone = new LambertZoneCal(zone);
            if (zone == LambertZone.Lambert93)
            {
                return GeographicToLambert(LatLon, lzone, LambertZoneCal.LON_MERID_IERS, LambertZoneCal.E_WGS84);
            }
            else
            {
                DVector3 pt1 = geographicToCartesian(LatLon.x - LambertZoneCal.LON_MERID_GREENWICH, LatLon.y, 0, LambertZoneCal.A_WGS84, LambertZoneCal.E_WGS84);

                pt1.translate(168, 60, -320);

                var pt2 = cartesianToGeographic(pt1, LambertZoneCal.LON_MERID_PARIS, LambertZoneCal.A_WGS84, LambertZoneCal.E_WGS84, LambertZoneCal.DEFAULT_EPS);
                var p = new DVector2(pt2.x, pt2.y);
                return GeographicToLambert(p, lzone, LambertZoneCal.LON_MERID_PARIS, LambertZoneCal.E_WGS84);
            }
        }
        private static double lambertNormal(double lat, double a, double e)
        {

            return a / Math.Sqrt(1 - e * e * Math.Sin(lat) * Math.Sin(lat));
        }

        private static DVector3 geographicToCartesian(double lon, double lat, double he, double a, double e)
        {
            double N = lambertNormal(lat, a, e);

            DVector3 pt = new DVector3(0, 0, 0);

            pt.x = (N + he) * Math.Cos(lat) * Math.Cos(lon);
            pt.y = (N + he) * Math.Cos(lat) * Math.Sin(lon);
            pt.z = (N * (1 - e * e) + he) * Math.Sin(lat);

            return pt;
        }

        private static DVector3 cartesianToGeographic(DVector3 org, double meridien, double a, double e, double eps)
        {
            double x = org.x, y = org.y, z = org.z;

            double lon = meridien + Math.Atan(y / x);

            double module = Math.Sqrt(x * x + y * y);

            double phi0 = Math.Atan(z / (module * (1 - (a * e * e) / Math.Sqrt(x * x + y * y + z * z))));
            double phiI = Math.Atan(z / module / (1 - a * e * e * Math.Cos(phi0) / (module * Math.Sqrt(1 - e * e * Math.Sin(phi0) * Math.Sin(phi0)))));
            double delta = Math.Abs(phiI - phi0);
            while (delta > eps)
            {
                phi0 = phiI;
                phiI = Math.Atan(z / module / (1 - a * e * e * Math.Cos(phi0) / (module * Math.Sqrt(1 - e * e * Math.Sin(phi0) * Math.Sin(phi0)))));
                delta = Math.Abs(phiI - phi0);

            }

            double he = module / Math.Cos(phiI) - a / Math.Sqrt(1 - e * e * Math.Sin(phiI) * Math.Sin(phiI));

            DVector3 pt = new DVector3(lon, phiI, he);

            return pt;
        }

        public static DVector3 convertToWGS84(DVector3 org, LambertZone zone)
        {

            var lzone = new LambertZoneCal(zone);

            if (zone == LambertZone.Lambert93)
            {
                return lambertToGeographic(org, lzone, LambertZoneCal.LON_MERID_IERS, LambertZoneCal.E_WGS84, LambertZoneCal.DEFAULT_EPS);
            }
            else
            {
                DVector3 pt1 = lambertToGeographic(org, lzone, LambertZoneCal.LON_MERID_PARIS, LambertZoneCal.E_CLARK_IGN, LambertZoneCal.DEFAULT_EPS);

                DVector3 pt2 = geographicToCartesian(pt1.x, pt1.y, pt1.z, LambertZoneCal.A_CLARK_IGN, LambertZoneCal.E_CLARK_IGN);

                pt2.translate(-168, -60, 320);

                //WGS84 refers to greenwich
                return cartesianToGeographic(pt2, LambertZoneCal.LON_MERID_GREENWICH, LambertZoneCal.A_WGS84, LambertZoneCal.E_WGS84, LambertZoneCal.DEFAULT_EPS);
            }
        }

        public static DVector3 convertToWGS84(double x, double y, LambertZone zone)
        {

            DVector3 pt = new DVector3(x, y, 0);
            return convertToWGS84(pt, zone);
        }

        public static DVector3 convertToWGS84Deg(double x, double y, LambertZone zone)
        {

            DVector3 pt = new DVector3(x, y, 0);
            pt = convertToWGS84(pt, zone);
            pt.toDegree();
            return pt;


        }

    }
    public class LambertZoneCal
    {

        private readonly static double[] LAMBERT_N = { 0.7604059656, 0.7289686274, 0.6959127966, 0.6712679322, 0.7289686274, 0.7256077650 };
        private readonly static double[] LAMBERT_C = { 11603796.98, 11745793.39, 11947992.52, 12136281.99, 11745793.39, 11754255.426 };
        private readonly static double[] LAMBERT_XS = { 600000.0, 600000.0, 600000.0, 234.358, 600000.0, 700000.0 };
        private readonly static double[] LAMBERT_YS = { 5657616.674, 6199695.768, 6791905.085, 7239161.542, 8199695.768, 12655612.050 };

        public readonly static double M_PI_2 = Math.PI / 2.0;
        public readonly static double DEFAULT_EPS = 1e-10;
        public readonly static double E_CLARK_IGN = 0.08248325676;
        public readonly static double E_WGS84 = 0.08181919106;

        public readonly static double A_CLARK_IGN = 6378249.2;
        public readonly static double A_WGS84 = 6378137.0;
        public readonly static double LON_MERID_PARIS = 0;
        public readonly static double LON_MERID_GREENWICH = 0.04079234433;
        public readonly static double LON_MERID_IERS = 3.0 * Math.PI / 180.0;

        public LambertZoneCal(LambertZone zone)
        {
            this.lambertZone = zone;
        }

        public double n()
        {
            return LAMBERT_N[(int)lambertZone];
        }

        public double c()
        {
            return LAMBERT_C[(int)lambertZone];
        }
        public double xs()
        {
            return LAMBERT_XS[(int)lambertZone];
        }
        public double ys()
        {
            return LAMBERT_YS[(int)lambertZone];
        }

        public LambertZone lambertZone { get; private set; }

    }
}