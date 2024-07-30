/*     Unity GIS Tech 2020-2021      */

using System;
using System.Linq;
using System.Collections;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderUTM
    {
        public GISTerrainLoaderUTM()
        {

        }
        public double[] UTMToLatLon(string UTM)
        {
            UTM2LatLon c = new UTM2LatLon();
            return c.convertUTMToLatLong(UTM);
        }

        public string LatLonToUTM(DVector2 LatLon)
        {
            LatLon2UTM c = new LatLon2UTM();
            return c.convertLatLonToUTM(LatLon.y, LatLon.x);
        }

        private static void validate(double latitude, double longitude)
        {
            if (latitude < -90.0 || latitude > 90.0 || longitude < -180.0 || longitude >= 180.0)
            {
                throw new Exception("Legal ranges: latitude [-90,90], longitude [-180,180).");
            }
        }

        public string LatLonToMGRUTM(DVector2 LatLon)
        {
            LatLon2MGRUTM c = new LatLon2MGRUTM();
            return c.convertLatLonToMGRUTM(LatLon.y, LatLon.x);
        }

        public double[] MGRUTMToLatLon(string MGRUTM)
        {
            MGRUTM2LatLon c = new MGRUTM2LatLon();
            return c.convertMGRUTMToLatLong(MGRUTM);
        }


        public static double degreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        public static double radianToDegree(double radian)
        {
            return radian * 180 / Math.PI;
        }

        private static double POW(double a, double b)
        {
            return Math.Pow(a, b);
        }

        private static double SIN(double value)
        {
            return Math.Sin(value);
        }

        private static double COS(double value)
        {
            return Math.Cos(value);
        }

        private static double TAN(double value)
        {
            return Math.Tan(value);
        }



        private class LatLon2UTM
        {
            public string convertLatLonToUTM(double latitude, double longitude)
            {
                validate(latitude, longitude);

                string UTM = "";

                setVariables(latitude, longitude);

                string longZone = getLongZone(longitude);
                LatZones latZones = new LatZones();
                string latZone = latZones.getLatZone(latitude);

                double _easting = getEasting();
                double _northing = getNorthing(latitude);

                UTM = longZone + " " + latZone + " " + ((int)_easting) + " " + ((int)_northing);

                return UTM;

            }

            protected void setVariables(double latitude, double longitude)
            {
                latitude = degreeToRadian(latitude);

                rho = equatorialRadius * (1 - e * e) / POW(1 - POW(e * SIN(latitude), 2), 3 / 2.0);

                nu = equatorialRadius / POW(1 - POW(e * SIN(latitude), 2), (1 / 2.0));

                double var1;
                if (longitude < 0.0)
                {
                    var1 = ((int)((180 + longitude) / 6.0)) + 1;
                }
                else
                {
                    var1 = ((int)(longitude / 6)) + 31;
                }
                double var2 = (6 * var1) - 183;
                double var3 = longitude - var2;
                p = var3 * 3600 / 10000;

                S = A0 * latitude - B0 * SIN(2 * latitude) + C0 * SIN(4 * latitude) - D0
                    * SIN(6 * latitude) + E0 * SIN(8 * latitude);

                K1 = S * k0;
                K2 = nu * SIN(latitude) * COS(latitude) * POW(sin1, 2) * k0 * (100000000)
                    / 2;
                K3 = ((POW(sin1, 4) * nu * SIN(latitude) * Math.Pow(COS(latitude), 3)) / 24)
                    * (5 - POW(TAN(latitude), 2) + 9 * e1sq * POW(COS(latitude), 2) + 4
                        * POW(e1sq, 2) * POW(COS(latitude), 4))
                    * k0
                    * (10000000000000000L);

                K4 = nu * COS(latitude) * sin1 * k0 * 10000;

                K5 = POW(sin1 * COS(latitude), 3) * (nu / 6)
                    * (1 - POW(TAN(latitude), 2) + e1sq * POW(COS(latitude), 2)) * k0
                    * 1000000000000L;

                A6 = (POW(p * sin1, 6) * nu * SIN(latitude) * POW(COS(latitude), 5) / 720)
                    * (61 - 58 * POW(TAN(latitude), 2) + POW(TAN(latitude), 4) + 270
                        * e1sq * POW(COS(latitude), 2) - 330 * e1sq
                        * POW(SIN(latitude), 2)) * k0 * (1E+24);

            }

            protected string getLongZone(double longitude)
            {
                double longZone = 0;
                if (longitude < 0.0)
                {
                    longZone = ((180.0 + longitude) / 6) + 1;
                }
                else
                {
                    longZone = (longitude / 6) + 31;
                }
                string val = ((int)longZone) + "";
                if (val.Length == 1)
                {
                    val = "0" + val;
                }
                return val;
            }

            protected double getNorthing(double latitude)
            {
                double northing = K1 + K2 * p * p + K3 * POW(p, 4);
                if (latitude < 0.0)
                {
                    northing = 10000000 + northing;
                }
                return northing;
            }

            protected double getEasting()
            {
                return 500000 + (K4 * p + K5 * POW(p, 3));
            }

            static double equatorialRadius = 6378137;

            static double polarRadius = 6356752.314;

             double rm = POW(equatorialRadius * polarRadius, 1 / 2.0);

            double k0 = 0.9996;

            static double e = Math.Sqrt(1 - POW(polarRadius / equatorialRadius, 2));

            double e1sq = e * e / (1 - e * e);

            double n = (equatorialRadius - polarRadius)
                / (equatorialRadius + polarRadius);

            // r curv 1
            double rho = 6368573.744;

            // r curv 2
            double nu = 6389236.914;

            // Calculate Meridional Arc Length
            // Meridional Arc
            double S = 5103266.421;

            double A0 = 6367449.146;

            double B0 = 16038.42955;

            double C0 = 16.83261333;

            double D0 = 0.021984404;

            double E0 = 0.000312705;

            // Calculation Constants
            // Delta Long
            double p = -0.483084;

            double sin1 = 4.84814E-06;

            // Coefficients for UTM Coordinates
            double K1 = 5101225.115;

            double K2 = 3750.291596;

            double K3 = 1.397608151;

            double K4 = 214839.3105;

            double K5 = -2.995382942;

            double A6 = -1.00541E-07;

        }
        private class LatLon2MGRUTM : LatLon2UTM
        {
            public string convertLatLonToMGRUTM(double latitude, double longitude)
            {
                validate(latitude, longitude);
                String mgrUTM = "";

                setVariables(latitude, longitude);

                string longZone = getLongZone(longitude);
                LatZones latZones = new LatZones();
                string latZone = latZones.getLatZone(latitude);

                double _easting = getEasting();
                double _northing = getNorthing(latitude);
                Digraphs digraphs = new Digraphs();
                string digraph1 = digraphs.getDigraph1(int.Parse(longZone),
                    _easting);
                string digraph2 = digraphs.getDigraph2(int.Parse(longZone),
                    _northing);

                string easting = ((int)_easting) + "";
                if (easting.Length < 5)
                {
                    easting = "00000" + easting;
                }
                easting = easting.Substring(easting.Length - 5);

                string northing;
                northing = ((int)_northing) + "";
                if (northing.Length < 5)
                {
                    northing = "0000" + northing;
                }
                northing = northing.Substring(northing.Length - 5);

                mgrUTM = longZone +" "  + latZone + " " + digraph1 + " " + digraph2 + " " + easting + " " + northing ;
                return mgrUTM;
            }
            public String convertLatLonToMGRUTM_Easting(double latitude, double longitude)
            {
                validate(latitude, longitude);
                String mgrUTM_Easting = "";

                setVariables(latitude, longitude);

                String longZone = getLongZone(longitude);
                LatZones latZones = new LatZones();
                String latZone = latZones.getLatZone(latitude);

                double _easting = getEasting();
                double _northing = getNorthing(latitude);
                Digraphs digraphs = new Digraphs();
                String digraph1 = digraphs.getDigraph1(int.Parse(longZone),
                        _easting);
                String digraph2 = digraphs.getDigraph2(int.Parse(longZone),
                        _northing);

                String easting = ((int)_easting).ToString(); ;
                if (easting.Count() < 5)
                {
                    easting = "00000" + easting;
                }
                easting = easting.Substring(easting.Count() - 5);

                String northing;
                northing = ((int)_northing).ToString(); ;
                if (northing.Count() < 5)
                {
                    northing = "0000" + northing;
                }
                northing = northing.Substring(northing.Count() - 5);

                mgrUTM_Easting = easting;
                return mgrUTM_Easting;
            }

            public String convertLatLonToMGRUTM_Northing(double latitude, double longitude)
            {
                validate(latitude, longitude);
                String mgrUTM_Northing = "";

                setVariables(latitude, longitude);

                String longZone = getLongZone(longitude);
                LatZones latZones = new LatZones();
                String latZone = latZones.getLatZone(latitude);

                double _easting = getEasting();
                double _northing = getNorthing(latitude);
                Digraphs digraphs = new Digraphs();
                String digraph1 = digraphs.getDigraph1(int.Parse(longZone),
                        _easting);
                String digraph2 = digraphs.getDigraph2(int.Parse(longZone),
                        _northing);

                String easting = ((int)_easting).ToString(); ;
                if (easting.Count() < 5)
                {
                    easting = "00000" + easting;
                }
                easting = easting.Substring(easting.Count() - 5);

                String northing;
                northing = ((int)_northing).ToString(); ;
                if (northing.Count() < 5)
                {
                    northing = "0000" + northing;
                }
                northing = northing.Substring(northing.Count() - 5);

                mgrUTM_Northing = northing;
                return mgrUTM_Northing;
            }

        }

        private class MGRUTM2LatLon : UTM2LatLon
        {
            public double[] convertMGRUTMToLatLong(string mgrutm)
            {
                double[] latlon = { 0.0, 0.0 };
                // 02CNR0634657742
                int zone = int.Parse(mgrutm.Substring(0, 2));
                string latZone = mgrutm.Substring(2, 3);

                string digraph1 = mgrutm.Substring(3, 4);
                string digraph2 = mgrutm.Substring(4, 5);
                easting = double.Parse(mgrutm.Substring(5, 10));
                northing = double.Parse(mgrutm.Substring(10, 15));

                LatZones lz = new LatZones();
                double latZoneDegree = lz.getLatZoneDegree(latZone);

                double a1 = latZoneDegree * 40000000 / 360.0;
                double a2 = 2000000 * Math.Floor(a1 / 2000000.0);

                Digraphs digraphs = new Digraphs();

                double digraph2Index = digraphs.getDigraph2Index(digraph2);

                double startindexEquator = 1;
                if ((1 + zone % 2) == 1)
                {
                    startindexEquator = 6;
                }

                double a3 = a2 + (digraph2Index - startindexEquator) * 100000;
                if (a3 <= 0)
                {
                    a3 = 10000000 + a3;
                }
                northing = a3 + northing;

                zoneCM = -183 + 6 * zone;
                double digraph1Index = digraphs.getDigraph1Index(digraph1);
                int a5 = 1 + zone % 3;
                double[] a6 = { 16, 0, 8 };
                double a7 = 100000 * (digraph1Index - a6[a5 - 1]);
                easting = easting + a7;

                setVariables();

                double latitude = 0;
                latitude = 180 * (phi1 - fact1 * (fact2 + fact3 + fact4)) / Math.PI;

                if (latZoneDegree < 0)
                {
                    latitude = 90 - latitude;
                }

                double d = _a2 * 180 / Math.PI;
                double longitude = zoneCM - d;

                if (getHemisphere(latZone).Equals("S"))
                {
                    latitude = -latitude;
                }

                latlon[0] = latitude;
                latlon[1] = longitude;
                return latlon;
            }
        }

        private class UTM2LatLon
        {
            public double easting;

            public double northing;

            public int zone;

            string southernHemisphere = "ACDEFGHJKLM";

            protected string getHemisphere(string latZone)
            {
                string hemisphere = "N";
                if (southernHemisphere.IndexOf(latZone) > -1)
                {
                    hemisphere = "S";
                }
                return hemisphere;
            }

            public double[] convertUTMToLatLong(string UTM)
            {
                double[] latlon = { 0.0, 0.0 };
                string[] utm = UTM.Split(' ');
                zone = int.Parse(utm[0]);
                string latZone = utm[1];
                easting = double.Parse(utm[2]);
                northing = double.Parse(utm[3]);
                string hemisphere = getHemisphere(latZone);
                double latitude = 0.0;
                double longitude = 0.0;

                if (hemisphere.Equals("S"))
                {
                    northing = 10000000 - northing;
                }
                setVariables();
                latitude = 180 * (phi1 - fact1 * (fact2 + fact3 + fact4)) / Math.PI;

                if (zone > 0)
                {
                    zoneCM = 6 * zone - 183.0;
                }
                else
                {
                    zoneCM = 3.0;

                }

                longitude = zoneCM - _a3;
                if (hemisphere.Equals("S"))
                {
                    latitude = -latitude;
                }

                latlon[0] = latitude;
                latlon[1] = longitude;
                return latlon;

            }

            protected void setVariables()
            {
                arc = northing / k0;
                mu = arc
                    / (a * (1 - POW(e, 2) / 4.0 - 3 * POW(e, 4) / 64.0 - 5 * POW(e, 6) / 256.0));

                ei = (1 - POW((1 - e * e), (1 / 2.0)))
                    / (1 + POW((1 - e * e), (1 / 2.0)));

                ca = 3 * ei / 2 - 27 * POW(ei, 3) / 32.0;

                cb = 21 * POW(ei, 2) / 16 - 55 * POW(ei, 4) / 32;
                cc = 151 * POW(ei, 3) / 96;
                cd = 1097 * POW(ei, 4) / 512;
                phi1 = mu + ca * SIN(2 * mu) + cb * SIN(4 * mu) + cc * SIN(6 * mu) + cd
                    * SIN(8 * mu);

                n0 = a / POW((1 - POW((e * SIN(phi1)), 2)), (1 / 2.0));

                r0 = a * (1 - e * e) / POW((1 - POW((e * SIN(phi1)), 2)), (3 / 2.0));
                fact1 = n0 * TAN(phi1) / r0;

                _a1 = 500000 - easting;
                dd0 = _a1 / (n0 * k0);
                fact2 = dd0 * dd0 / 2;

                t0 = POW(TAN(phi1), 2);
                Q0 = e1sq * POW(COS(phi1), 2);
                fact3 = (5 + 3 * t0 + 10 * Q0 - 4 * Q0 * Q0 - 9 * e1sq) * POW(dd0, 4)
                    / 24;

                fact4 = (61 + 90 * t0 + 298 * Q0 + 45 * t0 * t0 - 252 * e1sq - 3 * Q0
                    * Q0)
                    * POW(dd0, 6) / 720;

                //
                lof1 = _a1 / (n0 * k0);
                lof2 = (1 + 2 * t0 + Q0) * POW(dd0, 3) / 6.0;
                lof3 = (5 - 2 * Q0 + 28 * t0 - 3 * POW(Q0, 2) + 8 * e1sq + 24 * POW(t0, 2))
                    * POW(dd0, 5) / 120;
                _a2 = (lof1 - lof2 + lof3) / COS(phi1);
                _a3 = _a2 * 180 / Math.PI;

            }

            public double arc;

            public double mu;

            public double ei;

            public double ca;

            public double cb;

            public double cc;

            public double cd;

            public double n0;

            public double r0;

            public double _a1;

            public double dd0;

            public double t0;

            public double Q0;

            public double lof1;

            public double lof2;

            public double lof3;

            public double _a2;

            public double phi1;

            public double fact1;

            public double fact2;

            public double fact3;

            public double fact4;

            public double zoneCM;

            public double _a3;

            public double b = 6356752.314;

            public double a = 6378137;

            public double e = 0.081819191;

            public double e1sq = 0.006739497;

            public double k0 = 0.9996;

        }

        private class Digraphs
        {
            private Hashtable digraph1 = new Hashtable();

            private Hashtable digraph2 = new Hashtable();

            private string[] digraph1Array = { "A", "B", "C", "D", "E", "F", "G", "H",
        "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "W", "X",
        "Y", "Z" };

            private string[] digraph2Array = { "V", "A", "B", "C", "D", "E", "F", "G",
        "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V" };

            public Digraphs()
            {
                digraph1.Add("1", "A");
                digraph1.Add("2", "B");
                digraph1.Add("3", "C");
                digraph1.Add("4", "D");
                digraph1.Add("5", "E");
                digraph1.Add("6", "F");
                digraph1.Add("7", "G");
                digraph1.Add("8", "H");
                digraph1.Add("9", "J");
                digraph1.Add("10", "K");
                digraph1.Add("11", "L");
                digraph1.Add("12", "M");
                digraph1.Add("13", "N");
                digraph1.Add("14", "P");
                digraph1.Add("15", "Q");
                digraph1.Add("16", "R");
                digraph1.Add("17", "S");
                digraph1.Add("18", "T");
                digraph1.Add("19", "U");
                digraph1.Add("20", "V");
                digraph1.Add("21", "W");
                digraph1.Add("22", "X");
                digraph1.Add("23", "Y");
                digraph1.Add("24", "Z");

                digraph2.Add("0", "V");
                digraph2.Add("1", "A");
                digraph2.Add("2", "B");
                digraph2.Add("3", "C");
                digraph2.Add("4", "D");
                digraph2.Add("5", "E");
                digraph2.Add("6", "F");
                digraph2.Add("7", "G");
                digraph2.Add("8", "H");
                digraph2.Add("9", "J");
                digraph2.Add("10", "K");
                digraph2.Add("11", "L");
                digraph2.Add("12", "M");
                digraph2.Add("13", "N");
                digraph2.Add("14", "P");
                digraph2.Add("15", "Q");
                digraph2.Add("16", "R");
                digraph2.Add("17", "S");
                digraph2.Add("18", "T");
                digraph2.Add("19", "U");
                digraph2.Add("20", "V");

            }

            public int getDigraph1Index(String letter)
            {
                for (int i = 0; i < digraph1Array.Length; i++)
                {
                    if (digraph1Array[i].Equals(letter))
                    {
                        return i + 1;
                    }
                }

                return -1;
            }

            public int getDigraph2Index(String letter)
            {
                for (int i = 0; i < digraph2Array.Length; i++)
                {
                    if (digraph2Array[i].Equals(letter))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public String getDigraph1(int longZone, double easting)
            {
                int a1 = longZone;
                double a2 = 8 * ((a1 - 1) % 3) + 1;

                double a3 = easting;
                double a4 = a2 + ((int)(a3 / 100000)) - 1;
                return digraph1[((int)Math.Floor(a4)).ToString()].ToString();
            }

            public string getDigraph2(int longZone, double northing)
            {
                int a1 = longZone;
                double a2 = 1 + 5 * ((a1 - 1) % 2);
                double a3 = northing;
                double a4 = (a2 + ((int)(a3 / 100000)));
                a4 = (a2 + ((int)(a3 / 100000.0))) % 20;
                a4 = Math.Floor(a4);
                if (a4 < 0)
                {
                    a4 = a4 + 19;
                }
                return digraph1[((int)Math.Floor(a4)).ToString()].ToString();

            }

        }

        private class LatZones
        {
            private char[] letters = { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K',
        'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Z' };

            private int[] degrees = { -90, -84, -72, -64, -56, -48, -40, -32, -24, -16,
        -8, 0, 8, 16, 24, 32, 40, 48, 56, 64, 72, 84 };

            private char[] negLetters = { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K',
        'L', 'M' };

            private int[] negDegrees = { -90, -84, -72, -64, -56, -48, -40, -32, -24,
        -16, -8 };

            private char[] posLetters = { 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W',
        'X', 'Z' };

            private int[] posDegrees = { 0, 8, 16, 24, 32, 40, 48, 56, 64, 72, 84 };

            private int arrayLength = 22;

            public LatZones()
            {
            }

            public int getLatZoneDegree(string letter)
            {
                char ltr = letter.ToCharArray()[0];
                for (int i = 0; i < arrayLength; i++)
                {
                    if (letters[i] == ltr)
                    {
                        return degrees[i];
                    }
                }
                return -100;
            }

            public String getLatZone(double latitude)
            {
                int latIndex = -2;
                int lat = (int)latitude;

                if (lat >= 0)
                {
                    int len = posLetters.Length;
                    for (int i = 0; i < len; i++)
                    {
                        if (lat == posDegrees[i])
                        {
                            latIndex = i;
                            break;
                        }

                        if (lat > posDegrees[i])
                        {
                            continue;
                        }
                        else
                        {
                            latIndex = i - 1;
                            break;
                        }
                    }
                }
                else
                {
                    int len = negLetters.Length;
                    for (int i = 0; i < len; i++)
                    {
                        if (lat == negDegrees[i])
                        {
                            latIndex = i;
                            break;
                        }

                        if (lat < negDegrees[i])
                        {
                            latIndex = i - 1;
                            break;
                        }
                        else
                        {
                            continue;
                        }

                    }

                }

                if (latIndex == -1)
                {
                    latIndex = 0;
                }
                if (lat >= 0)
                {
                    if (latIndex == -2)
                    {
                        latIndex = posLetters.Length - 1;
                    }
                    return posLetters[latIndex] + "";
                }
                else
                {
                    if (latIndex == -2)
                    {
                        latIndex = negLetters.Length - 1;
                    }
                    return negLetters[latIndex] + "";

                }
            }

        }

    }


    public class UTMConvertor
    {



    }
 

}