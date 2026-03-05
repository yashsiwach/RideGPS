using MongoDB.Bson;
using MongoDB.Driver;
using NearbyFriendsApp.Models;

namespace NearbyFriendsApp.Services
{
    public class LocationService
    {
        private readonly IMongoCollection<UserLocation> _locations;
        private readonly IMongoCollection<RideRequest> _requests;

        public LocationService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDbSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDbSettings:DatabaseName"]);
            _locations = database.GetCollection<UserLocation>(config["MongoDbSettings:CollectionName"]);
            _requests = database.GetCollection<RideRequest>("RideRequests");
        }

        public void SaveLocation(UserLocation location)
        {
            var filter = Builders<UserLocation>.Filter.And(
                Builders<UserLocation>.Filter.Eq(x => x.Name, location.Name),
                Builders<UserLocation>.Filter.Eq(x => x.Role, location.Role)
            );

            var existing = _locations.Find(filter).FirstOrDefault();
            if (existing != null)
            {
                location.Id = existing.Id;
            }
            else if (string.IsNullOrEmpty(location.Id))
            {
                location.Id = ObjectId.GenerateNewId().ToString();
            }

            var options = new ReplaceOptions { IsUpsert = true };
            _locations.ReplaceOne(filter, location, options);
        }

        public List<UserLocation> GetAll()
        {
            return _locations.Find(x => true).ToList();
        }

        public List<UserLocation> GetRiders()
        {
            return _locations.Find(x => x.Role == "Rider").ToList();
        }

        public UserLocation? GetUserByName(string name, string role)
        {
            return _locations.Find(x => x.Name == name && x.Role == role).FirstOrDefault();
        }

        // Ride Request Methods
        public void CreateRideRequest(RideRequest request)
        {
            // Check if there's already a pending request from this customer
            var existingRequest = _requests.Find(x =>
                x.CustomerName == request.CustomerName &&
                x.Status == "Pending"
            ).FirstOrDefault();

            if (existingRequest != null)
            {
                // Preserve existing _id to avoid MongoDB immutability violation
                request.Id = existingRequest.Id;
                var filter = Builders<RideRequest>.Filter.Eq(x => x.Id, existingRequest.Id);
                _requests.ReplaceOne(filter, request);
            }
            else
            {
                _requests.InsertOne(request);
            }
        }

        public List<RideRequest> GetPendingRequestsForRider(string riderName)
        {
            return _requests.Find(x => x.RiderName == riderName && x.Status == "Pending").ToList();
        }

        public List<RideRequest> GetCustomerRequests(string customerName)
        {
            return _requests.Find(x => x.CustomerName == customerName)
                .SortByDescending(x => x.RequestedAt)
                .Limit(10)
                .ToList();
        }

        public RideRequest? GetRequestById(string requestId)
        {
            return _requests.Find(x => x.Id == requestId).FirstOrDefault();
        }

        public void UpdateRequestStatus(string requestId, string status)
        {
            var filter = Builders<RideRequest>.Filter.Eq(x => x.Id, requestId);
            var update = Builders<RideRequest>.Update
                .Set(x => x.Status, status)
                .Set(x => x.RespondedAt, DateTime.Now);
            _requests.UpdateOne(filter, update);
        }

        public RideRequest? GetActiveRequest(string customerName)
        {
            return _requests.Find(x =>
                x.CustomerName == customerName &&
                (x.Status == "Pending" || x.Status == "Accepted")
            ).SortByDescending(x => x.RequestedAt).FirstOrDefault();
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}