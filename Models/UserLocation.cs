using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NearbyFriendsApp.Models
{
    [BsonIgnoreExtraElements]
    public class UserLocation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        // New field to store if the user is a "Customer" or "Rider"
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}