using System;
using System.Collections.Generic;
using Demo.MedTech.DataModel.Shared;

namespace Demo.MedTech.Utility.Helper
{
    public static class IncrementHelper
    {
        /// <summary>
        /// Get Increment from increment range list for given amount
        /// </summary>
        /// <param name="increments">list of increment range</param>
        /// <param name="amount">User passed amount</param>
        /// <returns></returns>
        public static decimal GetIncrementFromRange(IReadOnlyList<Increment> increments, decimal amount)
        {
            decimal? relevantIncrement = null;

            for (int i = 0; i < increments.Count; i++)
            {
                Increment currentIncrementRange = increments[i];

                if (currentIncrementRange.Low >= 0 && currentIncrementRange.High > 0 &&
                    amount >= currentIncrementRange.Low && amount < currentIncrementRange.High)
                {
                    relevantIncrement = currentIncrementRange.IncrementValue;
                    break;
                }

                if (currentIncrementRange.Low >= 0 && currentIncrementRange.High == null &&
                    amount >= currentIncrementRange.Low)
                {
                    relevantIncrement = currentIncrementRange.IncrementValue ?? increments[i - 1].IncrementValue;
                    break;
                }
            }

            return Convert.ToDecimal(relevantIncrement);
        }
    }
}
