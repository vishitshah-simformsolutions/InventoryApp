using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Demo.MedTech.DataModel.Exceptions;

namespace Demo.MedTech.ValidationEngine.Model
{
    public class LotDetailRequest : LotDetail
    {
        private static readonly int IsValidDataTypeStatusCode = 101;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lotDetailRequest"></param>
        /// <param name="validateAuctionDetail"></param>
        /// <exception cref="RuleEngineException"></exception>
        public LotDetailRequest(IDictionary<string, JsonElement> lotDetailRequest, bool validateAuctionDetail = false)
        {
            var mismatchFieldsList = new List<string>();
            if (!validateAuctionDetail)
            {
                if (lotDetailRequest[nameof(base.LotId).ToLower()].ValueKind is JsonValueKind.Number && long.TryParse(Convert.ToString(lotDetailRequest[nameof(LotId).ToLower()]), out long _lotId))
                {
                    LotId = _lotId;
                }
                else
                {
                    mismatchFieldsList.Add(nameof(LotId));
                }
                var openingPriceString = lotDetailRequest[nameof(OpeningPrice).ToLower()];
                if (openingPriceString.ValueKind is JsonValueKind.Number && decimal.TryParse(Convert.ToString(openingPriceString), out decimal openingPrice))
                {
                    OpeningPrice = openingPrice;
                }
                else
                {
                    if (openingPriceString.ValueKind is JsonValueKind.Null)
                    {
                        OpeningPrice = null;
                    }
                    else
                    {
                        mismatchFieldsList.Add(nameof(OpeningPrice));
                    }
                }
                var reservePriceString = lotDetailRequest[nameof(ReservePrice).ToLower()];
                if (reservePriceString.ValueKind is JsonValueKind.Number && decimal.TryParse(Convert.ToString(reservePriceString), out var reservePrice))
                {
                    ReservePrice = reservePrice;
                }
                else
                {
                    if (reservePriceString.ValueKind is JsonValueKind.Null)
                    {
                        ReservePrice = null;
                    }
                    else
                    {
                        mismatchFieldsList.Add(nameof(ReservePrice));
                    }
                }
                
                var quantityString = lotDetailRequest[nameof(Quantity).ToLower()];
                if (quantityString.ValueKind is JsonValueKind.Number && decimal.TryParse(Convert.ToString(quantityString), out var quantity))
                {
                    Quantity = quantity;
                }
                else
                {
                    if (quantityString.ValueKind is JsonValueKind.Null)
                    {
                        Quantity = null;
                    }
                    else
                    {
                        mismatchFieldsList.Add(nameof(Quantity));
                    }
                }
            }

            if (lotDetailRequest[nameof(AuctionId).ToLower()].ValueKind is JsonValueKind.Number && long.TryParse(Convert.ToString(lotDetailRequest[nameof(AuctionId).ToLower()]), out long _auctionId))
            {
                AuctionId = _auctionId;
            }
            else
            {
                mismatchFieldsList.Add(nameof(AuctionId));
            }
            
            if ((object)lotDetailRequest[nameof(Increment).ToLower()] is List<Increment>)
            {
                Increment = (object)lotDetailRequest[nameof(Increment).ToLower()] as List<Increment>;
            }
            else
            {
                try
                {
                    Increment = JsonSerializer.Deserialize<List<Increment>>(Convert.ToString(lotDetailRequest[nameof(Increment).ToLower()]), JsonSerializerOption.CaseInsensitive);
                }
                catch
                {
                    if (decimal.TryParse(Convert.ToString(lotDetailRequest[nameof(Increment).ToLower()]), out decimal increment))
                    {
                        Increment = new List<Increment>
                        {
                            new Increment
                            {
                                Low = 0,
                                IncrementValue = increment
                            }
                        };
                    }
                    else
                    {
                        mismatchFieldsList.Add(nameof(Increment));
                    }
                }
            }
            
            string mismatchFields = string.Join(", ", mismatchFieldsList);
            if (string.IsNullOrEmpty(mismatchFields))
            {
                return;
            }
            //Need to uncomment when this properties need to use
            //ActiveBidsUrl = Convert.ToString(lotDetailRequest[nameof(ActiveBidsUrl).ToLower()])?.Trim();
            var ruleValidationMessage = new RuleValidationMessage() { IsValid = false };
            ruleValidationMessage.ValidationResults.Add(Response.PrepareValidationResult(IsValidDataTypeStatusCode, mismatchFields));

            throw new RuleEngineException(ruleValidationMessage);
        }
    }
}