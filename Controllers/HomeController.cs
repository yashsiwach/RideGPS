using Microsoft.AspNetCore.Mvc;
using NearbyFriendsApp.Models;
using NearbyFriendsApp.Services;
using System.Diagnostics;

namespace NearbyFriendsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly LocationService service;

        public HomeController(LocationService service)
        {
            this.service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveLocation([FromBody] UserLocation location)
        {
            if (location == null) return BadRequest();
            service.SaveLocation(location);
            return Ok(new { success = true });
        }

        [HttpGet]
        public IActionResult GetRiders()
        {
            var data = service.GetRiders();
            return Json(data);
        }

        [HttpGet]
        public IActionResult GetCustomers()
        {
            var data = service.GetCustomers();
            return Json(data);
        }

        [HttpPost]
        public IActionResult SendRideRequest([FromBody] RideRequestDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.CustomerName) || string.IsNullOrEmpty(dto.RiderName))
            {
                return BadRequest(new { error = "Invalid request data" });
            }

            var customer = service.GetUserByName(dto.CustomerName, "Customer");
            var rider = service.GetUserByName(dto.RiderName, "Rider");

            if (customer == null || rider == null)
            {
                return NotFound(new { error = "Customer or Rider not found" });
            }

            var distance = service.CalculateDistance(
                customer.Latitude, customer.Longitude,
                rider.Latitude, rider.Longitude
            );

            var request = new RideRequest
            {
                CustomerName = dto.CustomerName,
                CustomerLatitude = customer.Latitude,
                CustomerLongitude = customer.Longitude,
                RiderName = dto.RiderName,
                RiderLatitude = rider.Latitude,
                RiderLongitude = rider.Longitude,
                DistanceInKm = distance,
                Status = "Pending"
            };

            service.CreateRideRequest(request);
            return Ok(new { success = true, requestId = request.Id, distance = Math.Round(distance, 2) });
        }

        [HttpGet]
        public IActionResult GetPendingRequests(string riderName)
        {
            if (string.IsNullOrEmpty(riderName))
            {
                return BadRequest(new { error = "Rider name required" });
            }

            var requests = service.GetPendingRequestsForRider(riderName);
            return Json(requests);
        }

        [HttpGet]
        public IActionResult GetCustomerRequestStatus(string customerName)
        {
            if (string.IsNullOrEmpty(customerName))
            {
                return BadRequest(new { error = "Customer name required" });
            }

            var request = service.GetActiveRequest(customerName);
            return Json(request);
        }

        [HttpPost]
        public IActionResult RespondToRequest([FromBody] RespondRequestDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.RequestId) || string.IsNullOrEmpty(dto.Status))
            {
                return BadRequest(new { error = "Invalid request data" });
            }

            if (dto.Status != "Accepted" && dto.Status != "Rejected")
            {
                return BadRequest(new { error = "Status must be Accepted or Rejected" });
            }

            var request = service.GetRequestById(dto.RequestId);
            if (request == null)
            {
                return NotFound(new { error = "Request not found" });
            }

            service.UpdateRequestStatus(dto.RequestId, dto.Status);
            return Ok(new { success = true, status = dto.Status });
        }

        [HttpPost]
        public IActionResult CompleteRide([FromBody] CompleteRideDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.RequestId))
            {
                return BadRequest(new { error = "Invalid request data" });
            }

            service.UpdateRequestStatus(dto.RequestId, "Completed");
            return Ok(new { success = true });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class RideRequestDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string RiderName { get; set; } = string.Empty;
    }

    public class RespondRequestDto
    {
        public string RequestId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CompleteRideDto
    {
        public string RequestId { get; set; } = string.Empty;
    }
}