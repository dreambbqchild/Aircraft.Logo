using GeographicLib;
using Svg;
using Svg.Pathing;
using System.Drawing;

IReadOnlyList<PointF> GetData(string svg)
{
    var svgDocument = SvgDocument.Open(svg)!;
    var path = svgDocument.Children.OfType<SvgPath>().Single();
    return path.PathData.Where(pd => pd is not SvgClosePathSegment)
        .Select(pd => pd.End)
        .ToArray();
}

double MilesToMeters(double miles) 
    => miles * 1609.344;

double Distance(PointF start, PointF end) 
    => Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));

double RadiansToDegrees(double radians) 
    => (((radians > 0 ? radians : (2 * Math.PI + radians)) * 360.0 / (2 * Math.PI)) + 90) % 360.0;

string GetArgument(string arg, string? @default = null)
{
    var args = Environment.GetCommandLineArgs();
    var index = Array.IndexOf(args, $"-{arg}");
    if (index == -1)
    {
        if(@default is null)
            Environment.Exit(1);

        return @default;
    }

    if (index >= args.Length)
        Environment.Exit(2);

    return args[index + 1];
}

var strLat = GetArgument("lat"); //45d07'26"N
var strLon = GetArgument("lon"); //94d19'39"W
var svg = GetArgument("svg"); //file.svg
var mi = Double.Parse(GetArgument("miles", "15")); //15

var (lat, lon) = DMS.Decode(strLat, strLon);

var data = GetData(svg);
var yRange = data.Max(pt => pt.Y) - data.Min(pt => pt.Y);
var fpl = Garmin.FplFile.New(Path.GetFileNameWithoutExtension(svg));

fpl.AddPoint(lat, lon);

for (var i = 1; i < data.Count; i++)
{
    var prev = data[i - 1];
    var current = data[i];
    var rad = Math.Atan2(current.Y - prev.Y, current.X - prev.X);
    var deg = RadiansToDegrees(rad);
    var len = MilesToMeters((Distance(prev, current) / yRange) * mi);
    Geodesic.WGS84.Direct(lat, lon, deg, len, out lat, out lon);
    fpl.AddPoint(lat, lon);
}

fpl.Save($"{Path.GetFileNameWithoutExtension(svg)}.fpl");  