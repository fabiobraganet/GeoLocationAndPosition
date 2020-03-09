
namespace GeoLocationAndPosition.WebApp.API.V1
{
    using GeoLocationAndPosition.WebApp.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    [Route("api/v1/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {

        [HttpGet()]
        public async Task<List<Area>> GetAsync()
        {
            var filter = HttpContext.Request.Query;

            var areas = new List<Area>();

            for (int i = 0; i < 500; i++)
                areas.Add(new Area()
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Name = $"Area numero {i.ToString("00000000")}"
                });

            var name = filter["name"].FirstOrDefault();

            var result = areas
                .Where(p => string.IsNullOrEmpty(name) || p.Name.Contains(name))
                .ToList();

            return await Task.FromResult(result);
        }

        [HttpPost("")]
        public async Task PostAsync([FromForm]Area area)
        {
            if (area == null)
                throw new ArgumentNullException(nameof(area));

            if (area.Kml != null && !area.Kml.FileName.ToLower().EndsWith(".kml"))
                throw new Exception("Não é permitido arquivos que não são do tipo kml");

            await Task.CompletedTask;
        }

        [HttpPut("")]
        public async Task PutAsync([FromForm]Area area)
        {
            if (area == null || string.IsNullOrWhiteSpace(area.Id))
                throw new ArgumentNullException(nameof(area));

            if (area.Kml != null && !area.Kml.FileName.ToLower().EndsWith(".kml"))
                throw new Exception("Não é permitido arquivos que não são do tipo kml");

            if (area.Kml != null && area.Kml.Length > 0)
            {
                var kml = ReadKML(area.Kml);
            }

            await Task.CompletedTask;
        }

        [HttpDelete("")]
        public async Task DeleteAsync([FromForm]Area area)
        {
            await Task.CompletedTask;
        }



        private ICollection<Zone> ReadKML(IFormFile formFile)
        {
            XDocument kml;

            using (var stream = new MemoryStream())
            {
                formFile.CopyTo(stream);

                stream.Seek(0, SeekOrigin.Begin);

                kml = XDocument.Load(stream);
            }

            var @namespace = "{http://www.opengis.net/kml/2.2}";

            var placemarks = kml
                .Descendants($"{@namespace}Placemark")
                .Select(p => new
                {
                    Name = p.Descendants($"{@namespace}name").FirstOrDefault().Value,
                    Coordinates = p.Descendants($"{@namespace}coordinates").FirstOrDefault().Value
                        .Replace(" ", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace("0-", "-")
                        .Split(new string[] { ",0" }, StringSplitOptions.RemoveEmptyEntries)
                        .SelectMany(s => s.Split(','))
                        .ToList()

                })
                .ToList();

            var zones = new List<Zone>();

            foreach (var place in placemarks)
            {
                var latitudes = place.Coordinates.Where((c, i) => i % 2 == 1).ToList();
                var longitudes = place.Coordinates.Where((c, i) => i % 2 == 0).ToList();

                var coordinates = new List<Point>();

                var index = 0;

                foreach (var lat in latitudes)
                {
                    if (lat.TryParseDoubleReplacingSeparator(out var latitude) &&
                        longitudes[index].TryParseDoubleReplacingSeparator(out var longitude))

                        coordinates.Add(new Point(latitude, longitude));

                    index++;
                }

                var zone = new Zone(Guid.NewGuid(), place.Name);
                zone.Coordinates.AddRange(coordinates);

                zones.Add(zone);
            }

            return zones;
        }

    }

    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> owner, IEnumerable<T> values)
        {
            foreach (var value in values)
                owner.Add(value);
        }
    }

    public static class StringExtensions
    {
        public static bool TryParseDoubleReplacingSeparator(this string owner, out double value)
        {
            return double.TryParse(owner.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
    }
    
    
    public class Zone
    {
        public Zone()
        {
            Coordinates = new List<Point>();
        }

        public Zone(Guid zoneId, string zoneName) : this()
        {
            ZoneId = zoneId;
            ZoneName = zoneName;
        }

        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; }
        public ICollection<Point> Coordinates { get; set; }
    }

    //Point
    public class Point
    {
        public Point() { }

        public Point(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}