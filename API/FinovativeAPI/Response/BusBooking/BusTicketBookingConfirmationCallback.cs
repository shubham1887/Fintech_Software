using Newtonsoft.Json;
using Project.Processing;
using System;
using System.Collections.Generic;

namespace Project.Response.BusBooking
{
    public class BusTicketBookingConfirmationCallback
    {
        // public string @event { get; set; }

        private string _eventName;

        [JsonProperty("event")]
        public string eventName
        {
            get
            {
                return _eventName.TrimIfNotNull();
            }
            set
            {
                _eventName = value.TrimIfNotNull();
            }
        }

        public Param param { get; set; }
    }

    public class Param
    {
        public string amount { get; set; }
        public string base_fare { get; set; }
        public string comm { get; set; }
        public string tds { get; set; }
        public string total_deduction { get; set; }
        public string customer_mobile { get; set; }
        public string refid { get; set; }
        public string customer_email { get; set; }
        public string blockKey { get; set; }
        public string pnr_no { get; set; }
        public SeatDetails seat_details { get; set; }
    }

    public class SeatDetails
    {
        public string bookingFee { get; set; }
        public string busType { get; set; }
        public string cancellationCalculationTimestamp { get; set; }
        public string cancellationMessage { get; set; }
        public string cancellationPolicy { get; set; }
        public DateTime dateOfIssue { get; set; }
        public string destinationCity { get; set; }
        public string destinationCityId { get; set; }
        public DateTime doj { get; set; }
        public string dropLocation { get; set; }
        public string dropLocationAddress { get; set; }
        public string dropLocationId { get; set; }
        public string dropLocationLandmark { get; set; }
        public string dropTime { get; set; }
        public string firstBoardingPointTime { get; set; }
        public string hasRTCBreakup { get; set; }
        public string hasSpecialTemplate { get; set; }
        public string inventoryId { get; set; }
        public List<InventoryItem> inventoryItems { get; set; }
        public string MTicketEnabled { get; set; }
        public string partialCancellationAllowed { get; set; }
        public string pickUpContactNo { get; set; }
        public string pickUpLocationAddress { get; set; }
        public string pickupLocation { get; set; }
        public string pickupLocationId { get; set; }
        public string pickupLocationLandmark { get; set; }
        public string pickupTime { get; set; }
        public string pnr { get; set; }
        public string primeDepartureTime { get; set; }
        public string primoBooking { get; set; }
        public ReschedulingPolicy reschedulingPolicy { get; set; }
        public string serviceCharge { get; set; }
        public string sourceCity { get; set; }
        public string sourceCityId { get; set; }
        public string status { get; set; }
        public string tin { get; set; }
        public string travels { get; set; }
        public string vaccinatedBus { get; set; }
        public string vaccinatedStaff { get; set; }
    }

    public class InventoryItem
    {
        public string fare { get; set; }
        public string ladiesSeat { get; set; }
        public string malesSeat { get; set; }
        public string operatorServiceCharge { get; set; }
        public Passenger passenger { get; set; }
        public string seatName { get; set; }
        public string serviceTax { get; set; }
    }

    public class Passenger
    {
        public string address { get; set; }
        public string age { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string idNumber { get; set; }
        public string idType { get; set; }
        public string mobile { get; set; }
        public string name { get; set; }
        public string primary { get; set; }
        public string singleLadies { get; set; }
        public string title { get; set; }
    }

    public class ReschedulingPolicy
    {
        public string reschedulingCharge { get; set; }
        public string windowTime { get; set; }
    }
}