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
                ProductDetail = new ProductDetail
                {
                    ProductId = 1,
                    ItemId = 1,
                    SellingPrice = 10,
                    ManufacturingPrice = 100,
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
                }
            };
        }

        public static string CreateProductDetailString(long productId, long itemId, decimal? sellingPrice, decimal? buyItNow, decimal? quantity, string timeZone, int? extensionTimeInSeconds, decimal? manufacturingPrice = null, List<Increment> increments = null, bool? pieceMeal = false, string startTime = "2021-07-25T19:20:30+05:30", string endsFrom = "2022-07-27T19:20:30+05:30", string biddingType = "TimedBidding", string currencyType = "GBP", string incrementType = null, string reserveType = null,Guid AuctionHouseId = new Guid())
        {
            var userInput = new
            {
                ProductId = productId,
                ItemId = itemId,
                BiddingType = biddingType,
                SellingPrice = sellingPrice,
                ManufacturingPrice = manufacturingPrice,
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
     
        public static ProductDetail CreateProductDetail(long itemId,
            long productId,
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
                ProductId = productId,
                ItemId = itemId,
                SellingPrice = openingPrice,
                ManufacturingPrice = reservePrice,
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

        public static CreateLotRequestModelTest CreateRequestModel(ProductDetail productDetail)
        {
            return new CreateLotRequestModelTest
            {
                ProductDetail = productDetail
            };
        }

    }

    public class CreateLotRequestModelTest
    {
        public ProductDetail ProductDetail { get; set; }
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
