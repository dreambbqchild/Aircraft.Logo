using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Garmin
{
    internal class FplFile
    {        
        private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(FlightPlan));

        private readonly FlightPlan _flightPlan;
        private readonly List<Waypoint> _waypoints = new List<Waypoint>();
        private readonly List<RoutePoint> _routePoints = new List<RoutePoint>();

        private FplFile(FlightPlan flightPlan)
        {
            _flightPlan = flightPlan;
        }

        public void AddPoint(double lat, double lon)
        {
            var identifier = $"PT{_waypoints.Count:D3}";
            var strLat = GeographicLib.DMS.Encode(lat, 4, GeographicLib.HemisphereIndicator.Latitude, ' ').Replace(" ", string.Empty);
            var strLon = GeographicLib.DMS.Encode(lon, 4, GeographicLib.HemisphereIndicator.Longitude, ' ').Replace(" ", string.Empty);
            Console.Write($"{strLat}{strLon}  ");
            _waypoints.Add(new Waypoint()
            {
                identifier = identifier,
                lat = Convert.ToDecimal(lat),
                lon = Convert.ToDecimal(lon),
                type = WaypointType.USERWAYPOINT
            });

            _routePoints.Add(new RoutePoint()
            {
                waypointidentifier = identifier,
                waypointtype = WaypointType.USERWAYPOINT
            });
        }

        public void Save(string path)
        {
            _flightPlan.waypointtable = _waypoints.ToArray();
            _flightPlan.route.routepoint = _routePoints.ToArray();
            var xDoc = Serialize(_flightPlan);
            xDoc.Save(path);
        }

        private static FlightPlan Deserialize(XDocument doc)
        {
            using (var reader = doc.Root!.CreateReader())
                return (xmlSerializer.Deserialize(reader) as FlightPlan)!;
        }

        private static XDocument Serialize(FlightPlan value)
        {
            var doc = new XDocument();
            using (var writer = doc.CreateWriter())
                xmlSerializer.Serialize(writer, value);

            return doc;
        }

        public static FplFile Load(string path)
        {
            var xDoc = XDocument.Load(path);
            return new FplFile(Deserialize(xDoc));
        }

        public static FplFile New(string name)
        {
            var flightPlan = new FlightPlan();            
            flightPlan.route = new Route()
            {
                routename = name,
                flightplanindex = "1"
            };
            return new FplFile(flightPlan);
        }
    }
}
