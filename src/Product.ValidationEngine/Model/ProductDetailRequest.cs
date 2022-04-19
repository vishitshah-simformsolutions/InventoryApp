using Product.DataModel.Shared;
using Product.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Product.DataModel.Exceptions;

namespace Product.ValidationEngine.Model
{
    public class ProductDetailRequest : ProductDetail
    {
        private static readonly int IsValidDataTypeStatusCode = 101;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lotDetailRequest"></param>
        /// <param name="validateAuctionDetail"></param>
        /// <exception cref="RuleEngineException"></exception>
        public ProductDetailRequest(IDictionary<string, JsonElement> lotDetailRequest, bool validateAuctionDetail = false)
        {
            var mismatchFieldsList = new List<string>();
            if (!validateAuctionDetail)
            {
                if (lotDetailRequest[nameof(ItemId).ToLower()].ValueKind is JsonValueKind.Number && long.TryParse(Convert.ToString(lotDetailRequest[nameof(ItemId).ToLower()]), out long _lotId))
                {
                    ItemId = _lotId;
                }
                else
                {
                    mismatchFieldsList.Add(nameof(ItemId));
                }
                var sellingPriceString = lotDetailRequest[nameof(SellingPrice).ToLower()];
                if (sellingPriceString.ValueKind is JsonValueKind.Number && decimal.TryParse(Convert.ToString(sellingPriceString), out decimal openingPrice))
                {
                    SellingPrice = openingPrice;
                }
                else
                {
                    if (sellingPriceString.ValueKind is JsonValueKind.Null)
                    {
                        SellingPrice = null;
                    }
                    else
                    {
                        mismatchFieldsList.Add(nameof(SellingPrice));
                    }
                }
                var manufacturingPriceString = lotDetailRequest[nameof(ManufacturingPrice).ToLower()];
                if (manufacturingPriceString.ValueKind is JsonValueKind.Number && decimal.TryParse(Convert.ToString(manufacturingPriceString), out var reservePrice))
                {
                    ManufacturingPrice = reservePrice;
                }
                else
                {
                    if (manufacturingPriceString.ValueKind is JsonValueKind.Null)
                    {
                        ManufacturingPrice = null;
                    }
                    else
                    {
                        mismatchFieldsList.Add(nameof(ManufacturingPrice));
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

            if (lotDetailRequest[nameof(ProductId).ToLower()].ValueKind is JsonValueKind.Number && long.TryParse(Convert.ToString(lotDetailRequest[nameof(ProductId).ToLower()]), out long _auctionId))
            {
                ProductId = _auctionId;
            }
            else
            {
                mismatchFieldsList.Add(nameof(ProductId));
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