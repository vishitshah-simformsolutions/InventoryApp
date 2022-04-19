using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Demo.MedTech.DataModel.Shared
{
    public class LotModel
    {
        public LotDetail LotDetail { get; set; }
        public List<BiddingState> BiddingStates { get; set; }
        [JsonIgnore]
        public string ETag { get; set; }
    }

    public struct SchemaTypes
    {
        public const string CreateLot = "CreateLot";
        public const string EditLot = "EditLot";
    }

    public class Increment
    {
        public decimal Low { get; set; }
        public decimal? High { get; set; }
        public decimal? IncrementValue { get; set; }
    }
    public class LotDetail
    {
        public long AuctionId { get; set; }
        public long LotId { get; set; }
        public decimal? OpeningPrice { get; set; }
        public decimal? ReservePrice { get; set; }
        public List<Increment> Increment { get; set; }
        public decimal? Quantity { get; set; }
    }
   
    public class Action
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActorTypes ActorType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActionTypes ActionType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActionResults ActionResult { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public string Request { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RequestModels RequestModel { get; set; }
    }
    public enum RequestModels
    {
        CreateLotRequest = 1,
        EditLotRequest = 2,
        PlaceBidRequest = 3,
    }
    public enum ActionResults
    {
        LotCreated = 0,
        LotUpdated = 1,
        LotDeleted = 2,
    }
    public enum ActorTypes
    {
        System = 0,
        Bidder = 1,
        Auctioneer = 2,
        AuctionHouse = 3
    }
    public enum ActionTypes
    {
        LotUpdate = 0,
        CreateLot = 1,
        DeleteLot = 2,
    }
    public class State
    {
        public decimal MaxBid { get; set; }
        public string BidderId { get; set; }
        public decimal CurrentBid { get; set; }
        public decimal MinimumBid { get; set; }
    }
    public class BiddingState
    {
        public string Id { get; set; }
        public int SequenceNumber { get; set; }
        public Action Action { get; set; }
        public State State { get; set; }
    }
}