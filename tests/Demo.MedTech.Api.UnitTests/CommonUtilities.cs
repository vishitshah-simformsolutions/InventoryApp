using Product.DataModel.Shared;
using Product.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Product.UnitTests
{
    public static class CommonUtilities
    {
        public static ProductModel CreateLot()
        {
            return new ProductModel
            {
                LotDetail = new ProductDetail
                {
                    AuctionId = 1,
                    LotId = 1,
                    OpeningPrice = 10,
                    ReservePrice = 100,
                    Quantity = 5,
                    Increment = new List<Increment>()
                    {
                        new Increment()
                        {
                            Low = 0,
                            High = 100,
                            IncrementValue = 10
                        },
                        new Increment()
                        {
                            Low = 100,
                            High = 500,
                            IncrementValue = 50
                        },
                        new Increment()
                        {
                            Low = 500
                        }
                    }
                },
                BiddingStates = new List<BiddingState>
                {
                    new BiddingState
                    {
                        Id = "445939884",
                        Action = new Product.DataModel.Shared.Action
                        {
                            ActorType = ActorTypes.Bidder,
                            
                            ActionType = ActionTypes.CreateLot,
                            ActionResult = ActionResults.LotCreated,
                            TimeStamp = DateTime.UtcNow
                        },
                        SequenceNumber = 0,
                        State = new State
                        {
                            MaxBid = 0,
                            BidderId = "PBX54566",
                            CurrentBid = 0,
                            MinimumBid = 10,
                        }
                    }
                }
            };
        }

        public static string CreateLotDetailString(long AuctionId, long LotId, decimal? openingPrice, decimal? buyItNow, decimal? quantity, string timeZone, int? extensionTimeInSeconds, decimal? reservePrice = null, List<Increment> increments = null, bool? pieceMeal = false, string startTime = "2021-07-25T19:20:30+05:30", string endsFrom = "2022-07-27T19:20:30+05:30", string biddingType = "TimedBidding", string currencyType = "GBP", string incrementType = null, string reserveType = null,Guid AuctionHouseId = new Guid())
        {
            var userInput = new
            {
                AuctionId = AuctionId,
                LotId = LotId,
                BiddingType = biddingType,
                OpeningPrice = openingPrice,
                ReservePrice = reservePrice,
                BuyItNow = buyItNow,
                Currency = currencyType,
                IncrementType = incrementType,
                Increment = increments ?? new List<Increment> { new Increment { Low = 0, High = 50, IncrementValue = 5 }, new Increment { Low = 50, High = null, IncrementValue = 100 } },
                Quantity = quantity,
                IsPiecemeal = pieceMeal,
                StartTime = startTime,
                EndsFrom = endsFrom,
                TimeZone = timeZone,
                ExtensionTimeInSeconds = extensionTimeInSeconds,
                BiddingSuspended = false,
                ReserveType = reserveType,
                AuctionHouseId = AuctionHouseId
            };

            return JsonSerializer.Serialize(userInput);
        }

        public static decimal ConvertOffIncrementToOnIncrement(List<Increment> increments, decimal amount)
        {
            var incrementValue = IncrementHelper.GetIncrementFromRange(increments, amount);
            if (amount % incrementValue != 0)
            {
                amount = (Math.Floor(amount / incrementValue) * incrementValue);
            }

            return amount;
        }
     
        public static ProductDetail CreateLotDetail(long AuctionId,
            long LotId,
            decimal openingPrice,
            decimal? buyItNow,
            int quantity,
            string timeZone,
            int extensionTimeInSeconds,
            decimal? reservePrice = null,
            List<Increment> increments = null,
            DateTime startTime = default,
            DateTime endsFrom = default
        )
        {
            return new ProductDetail()
            {
                AuctionId = AuctionId,
                LotId = LotId,
                OpeningPrice = openingPrice,
                ReservePrice = reservePrice,
                Increment = increments ?? new List<Increment>
                {
                    new Increment {Low = 0,High = 50,IncrementValue = 5},
                    new Increment {Low = 50,High = 100,IncrementValue = 10},
                    new Increment {Low = 100,High = 500,IncrementValue = 25},
                    new Increment {Low = 500,High = 1000,IncrementValue = 50},
                    new Increment {Low = 1000,High = 5000,IncrementValue = 100},
                    new Increment {Low = 5000,High=null,IncrementValue = 100}
                },
                Quantity = quantity
            };
        }

        public static CreateLotRequestModelTest CreateRequestModel(ProductDetail lotDetail)
        {
            return new CreateLotRequestModelTest
            {
                LotDetail = lotDetail
            };
        }

    }

    public class CreateLotRequestModelTest
    {
        public ProductDetail LotDetail { get; set; }
    }
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

}
