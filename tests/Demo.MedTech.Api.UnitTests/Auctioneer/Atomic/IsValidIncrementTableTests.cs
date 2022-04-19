using System;
using System.Collections.Generic;
using System.Linq;
using Product.DataModel.Shared;
using Product.UnitTests;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;
using Product.ValidationEngine.Rules;
using Product.ValidationEngine.Rules.Auctioneer.Atomic;
using Xunit;

namespace Product.Api.UnitTests.Auctioneer.Atomic
{
    public class IsValidIncrementTableTests
    {
        private static readonly int IsValidIncrementTableErrorCode = 151;
        private readonly IList<IRule> _rules;
        private readonly IList<ITransform> _transformRules;
        private static IRequestPipe _requestPipe;

        public IsValidIncrementTableTests()
        {
            _requestPipe = new RequestPipe();
            _rules = typeof(IRule).Assembly.GetTypes()
                .Where(t => typeof(IRule).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as IRule).ToList();
            _transformRules = typeof(ITransform).Assembly.GetTypes()
                .Where(t => typeof(ITransform).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as ITransform).ToList();
        }

        private void AppendErrorDescription(ref RuleValidationMessage ruleValidationMessage, string[] descriptionKeyArray)
        {
            var errorDescription = new System.Text.StringBuilder();

            foreach (var key in descriptionKeyArray)
            {
                if (errorDescription.Length > 0)
                    errorDescription.Append("|");

                errorDescription.Append(Config.ErrorDescriptions.FirstOrDefault(x => x.Key.ToLower() == key.ToLower()).Value.Trim());
            }

            if (errorDescription.Length > 0)
            {
                var validationResult = new ValidationResult();
                var message = Response.ValidationResults.FirstOrDefault(x => x.Code == IsValidIncrementTableErrorCode);

                if (message != null)
                {
                    validationResult.Code = message.Code;
                    validationResult.Value = message.Value;
                }

                validationResult.Description = errorDescription.ToString();

                ruleValidationMessage.IsValid = false;
                ruleValidationMessage.ValidationResults.Add(validationResult);
            }
        }

        [Theory]
        [MemberData(nameof(TestInvalidIncrementData))]
        public void Given_increment_table_is_invalid_When_is_valid_increment_table_is_passed_Then_should_return_validation_error(List<Increment> increments, string[] expectedDescriptionKey)
        {
            //Arrange
            var expectedRuleValidationMessage = new RuleValidationMessage();
            ProductContext auctioneerContextContext = new ProductContext(CommonUtilities.CreateLotDetailString(1, 11, 35, 100, 1, "UTC", 60, null, increments), _requestPipe, _rules, _transformRules);
            AppendErrorDescription(ref expectedRuleValidationMessage, expectedDescriptionKey);

            //Act
            IsValidIncrementTable isValidIncrementTable = new IsValidIncrementTable();
            var result = isValidIncrementTable.Execute(auctioneerContextContext);

            //Assert
            Assert.False(result.IsValid);
            Assert.Equal(IsValidIncrementTableErrorCode, result.ValidationResults.FirstOrDefault()?.Code);
            Assert.Equal(expectedRuleValidationMessage.ValidationResults[0].Value,
                result.ValidationResults.FirstOrDefault()?.Value);

            Assert.Equal(expectedRuleValidationMessage.ValidationResults[0].Description,
                result.ValidationResults.FirstOrDefault()?.Description);
        }

        public static IEnumerable<object[]> TestInvalidIncrementData =>
            new List<object[]>
            {
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 0},
                        new Increment {Low = 100m, High = 500m, IncrementValue = 50},
                        new Increment {Low = 500m, High = null, IncrementValue = 100}
                    },
                    new[]{ "Increment_4"}
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 99.9999m, IncrementValue = 10},
                        new Increment {Low = 100.00m, High = 499.99m, IncrementValue = 50},
                        new Increment {Low = 500.19m, High = null, IncrementValue = 100}
                    },
                    new[]{ "Increment_1", "Increment_6" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 1899.99m, IncrementValue = 7},
                        new Increment {Low = 1900.00m, High =null, IncrementValue = null}
                    },
                    new[]{ "Increment_1", "Increment_6" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 10m, High = 99.9999m, IncrementValue = 10},
                        new Increment {Low = 100m, High =200m, IncrementValue = 20},
                        new Increment {Low = 200m, High =299.9999m, IncrementValue = 30}
                    },
                    new[]{ "Increment_1", "Increment_3", "Increment_6" , "Increment_9" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 10},
                        new Increment {Low = 100m, High =100m, IncrementValue = 20},
                        new Increment {Low = 100m, High =null, IncrementValue = 50},
                    },
                    new[]{ "Increment_2" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = null, IncrementValue = null}
                    },
                    new[]{ "Increment_5" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 10},
                        new Increment {Low = 100m, High = 500m, IncrementValue = null},
                        new Increment {Low = 500m, High = 1000m, IncrementValue = 50}
                    },
                    new[]{ "Increment_5" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 1000m, IncrementValue = 7}
                    },
                    new[]{ "Increment_7" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 0m, IncrementValue = 10}
                    },
                    new[]{ "Increment_2" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 10},
                        new Increment {Low = 100m, High =250m, IncrementValue = 20},
                        new Increment {Low = 250m, High =300m, IncrementValue = 30},
                    },
                    new[]{ "Increment_6" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 99.99m, IncrementValue = 10},
                        new Increment {Low = 100m, High =199.99m, IncrementValue = 20},
                        new Increment {Low = 200m, High =299.99m, IncrementValue = 40},
                    },
                    new[]{ "Increment_1","Increment_9" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 99.99m, IncrementValue = 10},
                        new Increment {Low = 100m, High =199.99m, IncrementValue = 20},
                        new Increment {Low = 200m, High =299.9999m, IncrementValue = 20},
                    },
                    new[]{ "Increment_1", "Increment_9" }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 99.999999m, IncrementValue = 0.000001m}
                    },
                    new[]{ "Increment_8"}
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 99.99m, IncrementValue = 10},
                        new Increment {Low = 100.00m, High = 499.999999999m, IncrementValue = 50},
                        new Increment {Low = 500.00m, High = null, IncrementValue = 100}
                    }
                    ,
                    new[]{  "Increment_1","Increment_8"}
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 10},
                        new Increment {Low = 100m, High = 499.99m, IncrementValue = 50},
                        new Increment {Low = 500.19m, High = null, IncrementValue = 100}
                    }
                    ,
                    new[]{  "Increment_1","Increment_6"}
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 99.99999m, IncrementValue = 10},
                        new Increment {Low = 100.00000m, High = 499.99999m, IncrementValue = 50},
                        new Increment {Low = 500.00000m, High = null, IncrementValue = 100}
                    }
                    ,
                    new[]{  "Increment_1"}
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 99.99m, IncrementValue = 10},
                        new Increment {Low = 100.00m, High = 499.99m, IncrementValue = 50},
                        new Increment {Low = 500.00m, High = null, IncrementValue = 100}
                    }
                    ,
                    new[]{  "Increment_1"}
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 500.000000000m, IncrementValue = 10}
                    }
                    ,
                    new[]{  "Increment_8"}
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low=0.00m,High=0.25m,IncrementValue=0.01m}
                        ,new Increment {Low=0.25m,High=0.50m,IncrementValue=0.05m}
                        ,new Increment {Low=0.50m,High=1.00m,IncrementValue=0.10m}
                        ,new Increment {Low=1.00m,High=5.00m,IncrementValue=0.25m}
                        ,new Increment {Low=5.00m,High=10.00m,IncrementValue=0.50m}
                        ,new Increment {Low=10.00m,High=25.00m,IncrementValue=1.00m}
                        ,new Increment {Low=25.00m,High=100.00m,IncrementValue=5.00m}
                        ,new Increment {Low=100.00m,High=250.00m,IncrementValue=10.00m}
                        ,new Increment {Low=250.00m,High=500.00m,IncrementValue=25.00m}
                        ,new Increment {Low=500.00m,High=1000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=1000.00m,High=5000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=5000.00m,High=25000.00m,IncrementValue=250.00m}
                        ,new Increment {Low=25000.00m,High=50000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=50000.00m,High=100000.00m,IncrementValue=1000.00m}
                        ,new Increment {Low=100000.00m,High=250000.00m,IncrementValue=2500.00m}
                        ,new Increment {Low=250000.00m,High=1000000.00m,IncrementValue=5000.00m}
                        ,new Increment {Low=1000000.00m,High=1100000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=1100000.00m,High=1200000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=1200000.00m,High=1300000.00m,IncrementValue=20.00m}
                        ,new Increment {Low=1300000.00m,High=1400000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=1400000.00m,High=1500000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=1500000.00m,High=1600000.00m,IncrementValue=200.00m}
                        ,new Increment {Low=1600000.00m,High=1700000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=1700000.00m,High=1800000.00m,IncrementValue=1000.00m}
                        ,new Increment {Low=1800000.00m,High=1900000.00m,IncrementValue=2000.00m}
                        ,new Increment {Low=1900000.00m,High=2000000.00m,IncrementValue=5000.00m}
                        ,new Increment {Low=2000000.00m,High=2100000.00m,IncrementValue=5000.00m}
                        ,new Increment {Low=2100000.00m,High=2200000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=2200000.00m,High=2300000.00m,IncrementValue=20.00m}
                        ,new Increment {Low=2300000.00m,High=2400000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=2400000.00m,High=2500000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=2500000.00m,High=2600000.00m,IncrementValue=200.00m}
                        ,new Increment {Low=2600000.00m,High=2700000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=2700000.00m,High=2800000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=2800000.00m,High=2900000.00m,IncrementValue=20.00m}
                        ,new Increment {Low=2900000.00m,High=3000000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=3000000.00m,High=3100000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=3100000.00m,High=3200000.00m,IncrementValue=200.00m}
                        ,new Increment {Low=3200000.00m,High=3300000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=3300000.00m,High=3400000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=3400000.00m,High=3500000.00m,IncrementValue=20.00m}
                        ,new Increment {Low=3500000.00m,High=3600000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=3600000.00m,High=3700000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=3700000.00m,IncrementValue=10000.00m}
                    }
                    ,
                    new[]{  "Increment_10"}
                }
            };

        [Theory]
        [MemberData(nameof(TestValidIncrementData))]
        public void Given_increment_table_is_valid_When_is_valid_increment_table_is_passed_Then_should_not_return_validation_error(List<Increment> increments)
        {
            //Arrange
            ProductContext auctioneerContextContext = new ProductContext(CommonUtilities.CreateLotDetailString(1, 11, 35, 100, 1, "UTC", 60, null, increments), _requestPipe, _rules, _transformRules);

            //Act
            IsValidIncrementTable isValidIncrementTable = new IsValidIncrementTable();
            var result = isValidIncrementTable.Execute(auctioneerContextContext);

            //Assert
            Assert.True(result.IsValid);
        }

        public static IEnumerable<object[]> TestValidIncrementData =>
            new List<object[]>
            {
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 10},
                        new Increment {Low = 100m, High = 500m, IncrementValue = 50},
                        new Increment {Low = 500m, High = null, IncrementValue = 100}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100.01m, IncrementValue = 0.010m},
                        new Increment {Low = 100.01m, High = 200m, IncrementValue = 0.01m},
                        new Increment {Low = 200m, High = null, IncrementValue = 10}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100.00001m, IncrementValue = 0.00001m},
                        new Increment {Low = 100.00001m, High = 500m, IncrementValue = 0.00001m},
                        new Increment {Low = 500m, High = null, IncrementValue = 10m}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = null, IncrementValue = 25}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 1000m, IncrementValue = 10},
                        new Increment {Low = 1000m, High =null, IncrementValue = null}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 10},
                        new Increment {Low = 100m, High = null, IncrementValue = 10}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 100m, IncrementValue = 10},
                        new Increment {Low = 100m, High =500, IncrementValue = 50},
                        new Increment {Low = 500m, High =null, IncrementValue = 100}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low = 0m, High = 500.00000m, IncrementValue = 10},
                        new Increment {Low = 500.00000m, High =null , IncrementValue = 10}
                    }
                },
                new object[]
                {
                     new List<Increment>
                     {
                        new Increment {Low=0.00m,High=0.25m,IncrementValue=0.01m}
                        ,new Increment {Low=0.25m,High=0.50m,IncrementValue=0.05m}
                        ,new Increment {Low=0.50m,High=1.00m,IncrementValue=0.10m}
                        ,new Increment {Low=1.00m,High=5.00m,IncrementValue=0.25m}
                        ,new Increment {Low=5.00m,High=10.00m,IncrementValue=0.50m}
                        ,new Increment {Low=10.00m,High=25.00m,IncrementValue=1.00m}
                        ,new Increment {Low=25.00m,High=100.00m,IncrementValue=5.00m}
                        ,new Increment {Low=100.00m,High=250.00m,IncrementValue=10.00m}
                        ,new Increment {Low=250.00m,High=500.00m,IncrementValue=25.00m}
                        ,new Increment {Low=500.00m,High=1000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=1000.00m,High=5000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=5000.00m,High=25000.00m,IncrementValue=250.00m}
                        ,new Increment {Low=25000.00m,High=50000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=50000.00m,High=100000.00m,IncrementValue=1000.00m}
                        ,new Increment {Low=100000.00m,High=250000.00m,IncrementValue=2500.00m}
                        ,new Increment {Low=250000.00m,High=1000000.00m,IncrementValue=5000.00m}
                        ,new Increment {Low=1000000.00m,IncrementValue=10000.00m}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low=0.00m,High=25.00m,IncrementValue=1.00m}
                        ,new Increment {Low=25.00m,High=100.00m,IncrementValue=5.00m}
                        ,new Increment {Low=100.00m,High=250.00m,IncrementValue=10.00m}
                        ,new Increment {Low=250.00m,High=500.00m,IncrementValue=25.00m}
                        ,new Increment {Low=500.00m,High=1000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=1000.00m,High=5000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=5000.00m,High=25000.00m,IncrementValue=250.00m}
                        ,new Increment {Low=25000.00m,High=100000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=100000.00m,High=null,IncrementValue=1000.00m}
                    }
                },
                new object[]
                {
                    new List<Increment>
                    {
                        new Increment {Low=0.00m,High=0.25m,IncrementValue=0.01m}
                        ,new Increment {Low=0.25m,High=0.50m,IncrementValue=0.05m}
                        ,new Increment {Low=0.50m,High=1.00m,IncrementValue=0.10m}
                        ,new Increment {Low=1.00m,High=5.00m,IncrementValue=0.25m}
                        ,new Increment {Low=5.00m,High=10.00m,IncrementValue=0.50m}
                        ,new Increment {Low=10.00m,High=25.00m,IncrementValue=1.00m}
                        ,new Increment {Low=25.00m,High=100.00m,IncrementValue=5.00m}
                        ,new Increment {Low=100.00m,High=250.00m,IncrementValue=10.00m}
                        ,new Increment {Low=250.00m,High=500.00m,IncrementValue=25.00m}
                        ,new Increment {Low=500.00m,High=1000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=1000.00m,High=5000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=5000.00m,High=25000.00m,IncrementValue=250.00m}
                        ,new Increment {Low=25000.00m,High=50000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=50000.00m,High=100000.00m,IncrementValue=1000.00m}
                        ,new Increment {Low=100000.00m,High=250000.00m,IncrementValue=2500.00m}
                        ,new Increment {Low=250000.00m,High=1000000.00m,IncrementValue=5000.00m}
                        ,new Increment {Low=1000000.00m,High=1100000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=1100000.00m,High=1200000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=1200000.00m,High=1300000.00m,IncrementValue=20.00m}
                        ,new Increment {Low=1300000.00m,High=1400000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=1400000.00m,High=1500000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=1500000.00m,High=1600000.00m,IncrementValue=200.00m}
                        ,new Increment {Low=1600000.00m,High=1700000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=1700000.00m,High=1800000.00m,IncrementValue=1000.00m}
                        ,new Increment {Low=1800000.00m,High=1900000.00m,IncrementValue=2000.00m}
                        ,new Increment {Low=1900000.00m,High=2000000.00m,IncrementValue=5000.00m}
                        ,new Increment {Low=2000000.00m,High=2010000.00m,IncrementValue=5000.00m}
                        ,new Increment {Low=2010000.00m,High=2020000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=2020000.00m,High=2030000.00m,IncrementValue=20.00m}
                        ,new Increment {Low=2030000.00m,High=2040000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=2040000.00m,High=2050000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=2050000.00m,High=2060000.00m,IncrementValue=200.00m}
                        ,new Increment {Low=2060000.00m,High=2070000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=2070000.00m,High=2080000.00m,IncrementValue=10.00m}
                        ,new Increment {Low=2080000.00m,High=2090000.00m,IncrementValue=20.00m}
                        ,new Increment {Low=2090000.00m,High=2100000.00m,IncrementValue=50.00m}
                        ,new Increment {Low=2100000.00m,High=2110000.00m,IncrementValue=100.00m}
                        ,new Increment {Low=2110000.00m,High=2120000.00m,IncrementValue=200.00m}
                        ,new Increment {Low=2120000.00m,High=2130000.00m,IncrementValue=500.00m}
                        ,new Increment {Low=2130000.00m,IncrementValue=10000.00m}
                    }
                }
            };
    }
}