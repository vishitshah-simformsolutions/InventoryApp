using System;
using System.Collections.Generic;

namespace SIgnalR.Model
{
    public class LotModel
    {
        public string domain { get; set; } = "SBS";
        public string subDomain { get; set; }
        public PublishDetails publishDetails { get; set; }
        public LotDetail lotDetail { get; set; }
        public List<BiddingState> biddingStates { get; set; }
    }

    public class PublishDetails
    {
        public bool isPremiumAuctionHouse { get; set; }
        public bool isPriorityEvent { get; set; }
        public string atgSourcePlatformCode { get; set; }
        public List<Platform> platforms { get; set; }
    }

    public class Platform
    {
        public string atgPlatformCode { get; set; }
        public Marketplace marketplace { get; set; }
    }

    public class Marketplace
    {
        public List<int> receiver { get; set; }
        public List<object> excluded { get; set; }
    }

    public class Increment
    {
        public decimal low { get; set; }
        public decimal? high { get; set; }
        public decimal? incrementValue { get; set; }
    }

    public class LotDetail
    {
        public long atgAuctionId { get; set; }
        public long atgLotId { get; set; }
        public BiddingTypes biddingType { get; set; }
        public decimal? openingPrice { get; set; }
        public decimal? reservePrice { get; set; }
        public decimal? buyItNow { get; set; }
        public string currency { get; set; }
        public string incrementType { get; set; }
        public List<Increment> increment { get; set; }
        public decimal? quantity { get; set; }
        public bool? isPiecemeal { get; set; }
        //should also handle epoc time, not required at the moment
        public DateTime startTime { get; set; }
        //should also handle epoc time, not required at the moment
        public DateTime endsFrom { get; set; }
        //ideally should use TimeZoneInfo as data type but not required at the moment
        public string timeZone { get; set; }
        public int? extensionTimeInSeconds { get; set; }
        public bool biddingSuspended { get; set; }
        public string reserveType { get; set; }
        public Guid atgAuctionHouseId { get; set; }
        public string activeBidsUrl { get; set; }
    }

    public struct ReserveTypes
    {
        public const string Standard = "Standard";
        public const string HouseBid = "HouseBid";
    }

    public enum BiddingTypes
    {
        TimedBidding = 0
    }

    public class Action
    {
        public ActorTypes actorType { get; set; }
        public string actorId { get; set; }
        public ActionTypes actionType { get; set; }
        public ActionResults actionResult { get; set; }
        public DateTime timestamp { get; set; } = DateTime.UtcNow;
        public string request { get; set; }
        public RequestModels requestModel { get; set; }
        public string atgPlatformCode { get; set; }
    }

    public enum RequestModels
    {
        PlaceBidRequest = 0,
        LotRequest = 1,
        NotApplicable = 2
    }

    public enum ActionResults
    {
        LotCreated = 0,
        BidPlaced = 1,
        MaxBidIncreased = 2,
        MaxBidDecreased = 3,
        LotStartTimeChanged = 4,
        LotEndTimeChanged = 5,
        LotBiddingEnded = 6,
        LotOpeningChanged = 7,
        LotMinimumBidUpdated = 8,
        LotReserveChanged = 9,
        ReserveStateUpdated = 10,
        LotIncrementChanged = 11,
        ReserveTypeChanged = 12,
        LotUpdated = 13
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
        SystemBid = 0,
        MaxBidSubmitted = 1,
        LotStartTimeChange = 2,
        LotEndTimeChange = 3,
        LotEndTimePast = 4,
        LotOpeningChange = 5,
        LotMinimumBidUpdate = 6,
        LotReserveChange = 7,
        ReserveStateUpdate = 8,
        LotIncrementChange = 9,
        HouseBid = 10,
        ReserveTypeChange = 11,
        LotUpdate = 12,
        CreateLot = 13
    }

    public class State
    {
        public decimal maxBid { get; set; }
        public string bidderId { get; set; }
        public string leadBidderId { get; set; }
        public string bidderRef { get; set; }
        public decimal currentBid { get; set; }
        public bool? isReservedMet { get; set; }
        public decimal minimumBid { get; set; }
        public DateTime endTime { get; set; }
        public int endTimeExtensionCount { get; set; }
        public int activeBids { get; set; }
        public string status { get; set; }
        public bool isTemporary { get; set; }
        public bool lotBiddingEnded { get; set; }
    }

    public class BiddingState
    {
        public string id { get; set; }
        public int sequenceNumber { get; set; }
        public Action action { get; set; }
        public State state { get; set; }
    }
}
