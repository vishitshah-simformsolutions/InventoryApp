﻿using Product.DataModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Action = Product.DataModel.Shared.Action;

namespace Product.Utility
{
    public static class AuctioneerUtility
    {
        public static JsonProperty GetPropertyCaseInsensitive(JsonElement jsonElement, string propName)
        {
            return jsonElement.EnumerateObject().FirstOrDefault(p => string.Compare(p.Name, propName,
                StringComparison.OrdinalIgnoreCase) == 0);
        }
       
        public static ProductModel AddDefaultState(dynamic lotDetail, ProductModel lotModel, string correlationId)
        {
            return new ProductModel
            {
                LotDetail = lotModel.LotDetail,
                BiddingStates = new List<BiddingState>
                {
                    new BiddingState
                    {
                        SequenceNumber = 0,
                        Id = correlationId,
                        Action = new Action
                        {
                            ActionResult = ActionResults.LotCreated,
                            ActionType = ActionTypes.CreateLot,
                            TimeStamp = DateTime.UtcNow,
                            ActorType = ActorTypes.AuctionHouse,
                            Request = JsonSerializer.Serialize(lotDetail),
                            RequestModel = RequestModels.CreateLotRequest
                        },
                        State = new State
                        {
                            CurrentBid = 0,
                            MaxBid = 0,
                            MinimumBid = lotModel.LotDetail.OpeningPrice.Value,
                            BidderId = null,
                        }
                    }
                },
            };
        }
    }
}
