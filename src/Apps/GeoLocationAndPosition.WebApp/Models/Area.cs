
namespace GeoLocationAndPosition.WebApp.Models
{
    using Microsoft.AspNetCore.Http;

    public class Area
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IFormFile Kml { get; set; }
    }

    public class AreaFilter
    {
        public string Name { get; set; }
    }
}
