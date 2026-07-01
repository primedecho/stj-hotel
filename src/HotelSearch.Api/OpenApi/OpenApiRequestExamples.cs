using System.Text.Json.Nodes;

namespace HotelSearch.Api.OpenApi;

internal static class OpenApiRequestExamples
{
    public static JsonNode? ForOperation(string? path, string? method) =>
        (path, method) switch
        {
            ("api/hotels", "POST") => CreateHotel,
            ("api/hotels/{id}", "PUT") => UpdateHotel,
            ("api/hotels/search", "POST") => SearchHotels,
            _ => null
        };

    private static JsonNode CreateHotel => JsonNode.Parse(
        """
        {
          "name": "Grand Hotel Zagreb",
          "price": 120,
          "latitude": 45.8150,
          "longitude": 15.9819
        }
        """)!;

    private static JsonNode UpdateHotel => JsonNode.Parse(
        """
        {
          "name": "Grand Hotel Zagreb",
          "price": 135,
          "latitude": 45.8150,
          "longitude": 15.9819
        }
        """)!;

    private static JsonNode SearchHotels => JsonNode.Parse(
        """
        {
          "prompt": "near 45.8150, 15.9819 under 200",
          "page": 1,
          "pageSize": 10
        }
        """)!;
}
