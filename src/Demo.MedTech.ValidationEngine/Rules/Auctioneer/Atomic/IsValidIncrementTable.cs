using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Utility.Helper;
using Demo.MedTech.ValidationEngine.Model;
using System.Collections.Generic;
using System.Linq;
using Demo.MedTech.Utility.Extension;

namespace Demo.MedTech.ValidationEngine.Rules.Auctioneer.Atomic
{
    /// <summary>
    /// Validate increment table
    /// </summary>
    public class IsValidIncrementTable : IRule
    {
        #region Constructor & Private variables

        private const int InvalidIncrementTableErrorCode = 151;
        private const int MaxDecimalPlacesAllowed = 5;

        #endregion

        public RuleValidationMessage Execute(ProductContext auctioneerContext)
        {
            int incrementTableLength = auctioneerContext.LotDetail.Increment.Count;
            List<Increment> incrementsList = auctioneerContext.LotDetail.Increment;
            Dictionary<int, string> result = new Dictionary<int, string>();

            if (incrementsList.Any())
            {
                if (IsIncrementTableRowsOutOfRange(incrementsList))
                {
                    result.Add(10, GetErrorDescription(10));
                }
                if (IsFirstLowerLimitZero(incrementsList[0]))
                {
                    result.Add(3, GetErrorDescription(3));
                }

                if (incrementTableLength == 1)
                {
                    if (IsIncrementNullForOnlyOneRange(incrementsList[0]))
                    {
                        result.Add(5, GetErrorDescription(5));
                    }

                    if (IsIncrementZeroForOnlyOneRange(incrementsList[0]))
                    {
                        result.Add(4, GetErrorDescription(4));
                    }

                    if (IsFirstHigherLimitNotDivisibleByIncrement(incrementsList[0]))
                    {
                        result.Add(7, GetErrorDescription(7));
                    }
                }

                for (int i = 0; i < incrementTableLength; i++)
                {
                    Increment previousRange = i == 0 ? null : incrementsList[i - 1];
                    Increment currentRange = incrementsList[i];
                    bool isLastRange = (i == incrementTableLength - 1);

                    if (!result.ContainsKey(2))
                    {
                        if (IsLowerAndHigherValueSame(currentRange))
                        {
                            result.Add(2, GetErrorDescription(2));
                        }
                    }

                    if (!result.ContainsKey(5))
                    {
                        if (IsIncrementNull(currentRange, isLastRange))
                        {
                            result.Add(5, GetErrorDescription(5));
                        }
                    }

                    if (!result.ContainsKey(4))
                    {
                        if (IsIncrementLessThanEqualZero(currentRange, isLastRange))
                        {
                            result.Add(4, GetErrorDescription(4));
                        }
                    }

                    if (!result.ContainsKey(6))
                    {
                        if (IsLowerNotDivisibleByCurrentAndPreviousIncrement(previousRange, currentRange, isLastRange))
                        {
                            result.Add(6, GetErrorDescription(6));
                        }
                    }

                    if (!result.ContainsKey(9))
                    {
                        if (IsPossibleNextLowerNotDivisibleByCurrentAndPreviousIncrement(previousRange, currentRange, isLastRange))
                        {
                            result.Add(9, GetErrorDescription(9));
                        }
                    }

                    if (!result.ContainsKey(8))
                    {
                        if (IsIncrementRangeContainMoreThanDefinedDecimalPlaces(currentRange))
                        {
                            result.Add(8, GetErrorDescription(8));
                        }
                    }

                    if (i <= 0) continue;
                    if (!result.ContainsKey(1))
                    {
                        if (CheckRangeIsConsecutive(previousRange, currentRange))
                        {
                            result.Add(1, GetErrorDescription(1));
                        }
                    }
                }
            }
            else
            {
                /// The increment table should have more than 0 row(s)
                result.Add(11, GetErrorDescription(11));
            }

            return AddValidationMessage(result);
        }

        #region Increment validation function


        /// <summary>
        /// a. The range must be consecutive
        /// The previous row's higher range should be equal to the next row's lower range
        /// </summary>
        /// <param name="previousRange"></param>
        /// <param name="currentRange"></param>
        private bool CheckRangeIsConsecutive(Increment previousRange, Increment currentRange)
        {
            // Check if current range is consecutive to previous range
            return currentRange.Low != previousRange.High;
        }

        /// <summary>
        /// Max 5 decimal places are allowed
        /// </summary>
        /// <param name="currentRange"></param>
        private bool IsIncrementRangeContainMoreThanDefinedDecimalPlaces(Increment currentRange)
        {
            //If there any value contain more than 5 decimal places
            return currentRange.Low.GetDecimalPlaces() > MaxDecimalPlacesAllowed ||
                   (currentRange.High.HasValue && currentRange.High.Value.GetDecimalPlaces() > MaxDecimalPlacesAllowed) ||
                   (currentRange.IncrementValue.HasValue && currentRange.IncrementValue.Value.GetDecimalPlaces() > MaxDecimalPlacesAllowed);
        }

        /// <summary>
        /// b. The lower and higher values for the same row cannot be equal. (must have positive difference)
        /// </summary>
        /// <param name="currentRange"></param>
        private bool IsLowerAndHigherValueSame(Increment currentRange)
        {
            return currentRange.Low >= currentRange.High;
        }

        /// <summary>
        /// c. The lower limit ​for the first row must be zero.
        /// </summary>
        /// <param name="currentRange"></param>
        private bool IsFirstLowerLimitZero(Increment currentRange)
        {
            return currentRange.Low != 0;
        }

        /// <summary>
        /// d. The increment value cannot be zero.
        /// If only one row exists then the increment value cannot be null [No. of rows = 1]
        /// </summary>
        /// <param name="currentRange"></param>
        private bool IsIncrementNullForOnlyOneRange(Increment currentRange)
        {
            //If there is only one range then increment can not be null or less than 0
            return !currentRange.IncrementValue.HasValue;
        }

        private bool IsIncrementZeroForOnlyOneRange(Increment currentRange)
        {
            //If there is only one range then increment can not be null or less than 0
            return currentRange.IncrementValue <= 0;
        }

        /// <summary>
        /// d. The increment value cannot be zero.
        /// </summary>
        /// <param name="currentRange"></param>
        /// <param name="isLastRange"></param>
        private bool IsIncrementLessThanEqualZero(Increment currentRange, bool isLastRange)
        {
            decimal? incrementValue = currentRange.IncrementValue;
            if (!isLastRange)
            {
                if (incrementValue <= 0)
                {
                    return true;
                }
            }

            //This case if for range like [low = someValue, High = null, Increment = can be null, and if it has then should be >0]
            // e. Increment value cannot be null unless it is the last row of the increment table  [No. of rows > 1]
            return isLastRange && incrementValue <= 0;
        }

        private bool IsIncrementNull(Increment currentRange, bool isLastRange)
        {
            decimal? incrementValue = currentRange.IncrementValue;
            if (!isLastRange)
            {
                if (!incrementValue.HasValue)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// f. If the increment value is not null then, the lower value should be divisible by:
        /// Increment ​set for a given range AND
        /// The increment set for the previous range
        /// </summary>
        /// <param name="previousRange"></param>
        /// <param name="currentRange"></param>
        /// <param name="isLastRange"></param>
        private bool IsLowerNotDivisibleByCurrentAndPreviousIncrement(Increment previousRange, Increment currentRange, bool isLastRange)
        {
            if (!isLastRange && !currentRange.IncrementValue.HasValue || currentRange.IncrementValue == 0)
            {
                return false;
            }

            if (previousRange != null && (!previousRange.IncrementValue.HasValue || previousRange.IncrementValue == 0))
            {
                return false;
            }

            //If there is only one row in increment table, or this is the very first range
            if (previousRange == null && currentRange.IncrementValue.HasValue && currentRange.Low % currentRange.IncrementValue != 0)
            {
                return true;
            }

            if (!isLastRange && previousRange != null && (currentRange.Low % currentRange.IncrementValue != 0 || currentRange.Low % previousRange.IncrementValue != 0))
            {
                return true;
            }

            // g. If the increment value is null, then lower value divisibility with Increment is not required.
            //This is for the last range when there is multiple range in increment table
            if (isLastRange && previousRange != null && currentRange.Low % previousRange.IncrementValue != 0)
            {
                return true;
            }
            if (isLastRange && previousRange != null && currentRange.IncrementValue.HasValue && currentRange.Low % currentRange.IncrementValue != 0)
            {
                return true;
            }

            return false;
        }

        private bool IsPossibleNextLowerNotDivisibleByCurrentAndPreviousIncrement(Increment previousRange, Increment currentRange, bool isLastRange)
        {
            if (!isLastRange && !currentRange.IncrementValue.HasValue || currentRange.IncrementValue == 0)
            {
                return false;
            }

            if (previousRange != null && (!previousRange.IncrementValue.HasValue || previousRange.IncrementValue == 0))
            {
                return false;
            }

            // After transformation the possible lower value of added increment range is not divisible by the increment value
            if (isLastRange && previousRange != null && currentRange.High.HasValue)
            {
                decimal possibleNexLow = currentRange.High.Value;

                if (currentRange.IncrementValue.HasValue && possibleNexLow % currentRange.IncrementValue != 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// h. If the increment table has only one row AND higher value is equal to null, then any increment higher than zero is accepted
        /// i. If the increment table has only one row AND higher value is not null, then the increment value must be divisible by the higher value
        /// </summary>
        /// <param name="currentRange"></param>
        private bool IsFirstHigherLimitNotDivisibleByIncrement(Increment currentRange)
        {
            decimal? incrementValue = currentRange.IncrementValue;
            if (incrementValue == 0)
            {
                return false;
            }

            //When there is just one range and Higher value is not null, then the Higher value must be divisible by the Increment value
            return currentRange.High.HasValue && currentRange.High % incrementValue != 0;
        }

        private bool IsIncrementTableRowsOutOfRange(List<Increment> incrementList)
        {
            return incrementList.Count > 40;
        }

        #endregion

        #region Other members

        /// <summary>
        /// Add increment validation descriptions
        /// </summary>
        private RuleValidationMessage AddValidationMessage(Dictionary<int, string> result)
        {
            RuleValidationMessage ruleValidationMessage = new RuleValidationMessage() { IsValid = true };

            if (result.Count == 0)
            {
                return ruleValidationMessage;
            }
            var errorDescription = string.Join("|", result.OrderBy(x => x.Key).Select(y => y.Value));

            var validationResult = new ValidationResult();
            var message = Response.ValidationResults.FirstOrDefault(x => x.Code == InvalidIncrementTableErrorCode);

            if (message == null)
            {
                return ruleValidationMessage;
            }
            validationResult.Code = message.Code;
            validationResult.Value = message.Value;

            validationResult.Description = errorDescription.ToString();

            ruleValidationMessage.IsValid = false;
            ruleValidationMessage.ValidationResults.Add(validationResult);

            return ruleValidationMessage;
        }

        private string GetErrorDescription(int key)
        {
            string descriptionKey = "Increment_" + key;
            return Config.ErrorDescriptions[descriptionKey].Trim();
        }


        #endregion
    }
}