using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NearbyFriendsApp.Models
{
    [BsonIgnoreExtraElements]
    public class RideRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public double CustomerLatitude { get; set; }

        public double CustomerLongitude { get; set; }

        public string RiderName { get; set; } = string.Empty;

        public double RiderLatitude { get; set; }

        public double RiderLongitude { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Completed

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public DateTime? RespondedAt { get; set; }

        public double DistanceInKm { get; set; }
    }
}
