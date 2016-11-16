using System;

namespace Yavsc.Models.Market {
    /// <summary>
 /// Not yet used!
 /// </summary>
 public class Money : IUnit<decimal>
    {
        public Money(string name, decimal euroExchangeRate)
        {
            Name = name;
            EuroExchangeRate = euroExchangeRate;
        }

        public string Name
        {
            get; private set ;
        }

        public decimal EuroExchangeRate
        {
             get; private set ;
        }
        public bool CanConvertFrom(IUnit<decimal> other)
        {
            if (other is Money)
                  return true;
            return false;
        }

        public decimal ConvertFrom(IUnit<decimal> other, decimal orgValue)
        {
            if (other is Money) {
                var om = other as Money;
                return orgValue * om.EuroExchangeRate / EuroExchangeRate;
            }
            throw new NotImplementedException();
        }
    }
 }