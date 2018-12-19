using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BingMapsRESTToolkit;

namespace Geocoding
{
    public class Lookup
    {

        public static async Task<MEPModel.Location> reversegeocode(double latitude, double longitude)
        {
            BingMapsRESTToolkit.Location result = null;
            ReverseGeocodeRequest request = new ReverseGeocodeRequest()
            {
                Point = new Coordinate(latitude, longitude),
                BingMapsKey = Settings1.Default.BingKey
            };
            var response = await ServiceManager.GetResponseAsync(request);
            if (response != null &&
               response.ResourceSets != null &&
               response.ResourceSets.Length > 0 &&
               response.ResourceSets[0].Resources != null &&
               response.ResourceSets[0].Resources.Length > 0)
            {

                result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;
                //this.searchlocation = 
                //var b = searchlocation.BoundingBox;
                //myMap.SetView(new Microsoft.Maps.MapControl.WPF.LocationRect(b[0], b[1], b[2], b[3]));
            }
            return new MEPModel.Location(latitude, longitude, result.Address.AddressLine, result.Address.PostalCode, result.Address.Locality, result.Address.CountryRegion);
        }


        public static async Task<MEPModel.Location> geocode(String address)
        {
            BingMapsRESTToolkit.Location result = null;
            GeocodeRequest request = new GeocodeRequest()
            {
                Query = address,
                IncludeIso2 = true,
                IncludeNeighborhood = true,
                MaxResults = 25,
                BingMapsKey = Settings1.Default.BingKey
            };
            var response = await ServiceManager.GetResponseAsync(request);
            if (response != null &&
               response.ResourceSets != null &&
               response.ResourceSets.Length > 0 &&
               response.ResourceSets[0].Resources != null &&
               response.ResourceSets[0].Resources.Length > 0)
            {

                result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;
                //this.searchlocation = 
                //var b = searchlocation.BoundingBox;
                //myMap.SetView(new Microsoft.Maps.MapControl.WPF.LocationRect(b[0], b[1], b[2], b[3]));
            }
            return new MEPModel.Location(result.Point.Coordinates[0], result.Point.Coordinates[1], result.Address.AddressLine, result.Address.PostalCode, result.Address.Locality, result.Address.CountryRegion);
        
        
        }
    }
}
