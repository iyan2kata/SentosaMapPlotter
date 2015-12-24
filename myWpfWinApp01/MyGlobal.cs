using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using myWpfWinApp01.Models;
using Microsoft.Maps.MapControl.WPF;

namespace myWpfWinApp01
{
    public class MyGlobal
    {

        //Calculate distance between 2 Locations
        public static double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            double distance = 0;

            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                        + Math.Cos(lat1 / 180 * Math.PI) * Math.Cos(lat2 / 180 * Math.PI)
                        * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //Calculate radius of earth
            // For this you can assume any of the two points.
            double radiusE = 6378135; // Equatorial radius, in metres
            double radiusP = 6356750; // Polar Radius

            //Numerator part of function
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            //Denominator part of the function
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                            + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);

            //Calculate distance in meters.
            distance = radius * c;
            return distance; // distance in meters
        }

        //In km/hour
        public static double GetEstimatedTime(double speed, double distance)
        {
            if (speed > 0)
            {
                //return ((distance/1000)/speed)*60;

                //Calculate based on Current Map Plotter
                return ((distance) / speed) * 60 * 60;
            }
            else
            {
                return 0;
            }
        }

        //Getting the Image Icon
        public static string GetStrImageUri(string sIconType)
        {
            string strImgUri = "http://icons.iconarchive.com/icons/fatcow/farm-fresh/32/walk-icon.png";
            switch (sIconType.ToLower())
            {
                case "walk":
                    strImgUri = "http://icons.iconarchive.com/icons/fatcow/farm-fresh/32/walk-icon.png";
                    break;
                case "bus":
                    strImgUri =
                        "http://icons.iconarchive.com/icons/iconshock/real-vista-transportation/32/school-bus-icon.png";
                    break;
                case "tram":
                    strImgUri = "http://icons.iconarchive.com/icons/awicons/vista-artistic/32/2-Hot-Train-icon.png";
                    break;
                case "rail":
                    strImgUri =
                        "http://icons.iconarchive.com/icons/3xhumed/mega-games-pack-28/32/Rail-Simulator-2-icon.png";
                    break;
                case "poi":
                    strImgUri =
                        "http://icons.iconarchive.com/icons/proycontec/beach/32/sand-castle-icon.png";
                    break;
                case "rest":
                    strImgUri =
                        "http://icons.iconarchive.com/icons/icons-land/points-of-interest/32/Restaurant-Blue-icon.png";
                    break;

                case "cable":
                    strImgUri =
                        "http://icons.iconarchive.com/icons/elegantthemes/beautiful-flat-one-color/32/car-icon.png";
                    break;

                case "cableway":
                    strImgUri = "http://icons.iconarchive.com/icons/3xhumed/mega-games-pack-31/32/Cars-pixar-4-icon.png";
                    break;

                case "node":
                    strImgUri = "http://icons.iconarchive.com/icons/hopstarter/soft-scraps/128/Button-Blank-Gray-icon.png";
                    break;
                default:
                    break;
            }
            return strImgUri;
        }

        //Getting Stroke Color
        public static Color GetPathColor(string pathType)
        {
            Color result;
            switch (pathType.ToLower())
            {
                case "walk":
                    result = Colors.Red;
                    break;
                case "bus":
                case "bus-d":
                    result = Colors.Blue;
                    break;
                case "tram":
                    result = Colors.LimeGreen;
                    break;
                case "rail":
                    result = Colors.Yellow;
                    break;
                case "cable":
                    result = Colors.CornflowerBlue;
                    break;
                default:
                    result = Colors.Red;
                    break;
            }
            return result;
        }

        //Get Speed base on NodeType
        //'TRAM':20000,'BUS':40000,'WALK':2000,'RAIL':80000,'CABLE':160000
        public static double GetTheSpeed(string nodeType)
        {
            double result = 0.0;
            switch (nodeType.ToLower())
            {
                case "walk":
                    result = 2000;
                    break;
                case "bus":
                case "bus-d":
                    result = 40000;
                    break;
                case "tram":
                    result = 20000;
                    break;
                case "rail":
                    result = 80000;
                    break;
                case "cable":
                    result = 160000;
                    break;
                case "cableway":
                    result = 160000;
                    break;
                default:
                    break;
            }
            return result;
        }

    }
}
