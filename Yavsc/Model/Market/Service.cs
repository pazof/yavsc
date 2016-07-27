
namespace Yavsc.Models.Market {
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public enum BillingMode { 
        Unitary,
        SetPrice,
        ByExecutionTime
    }

    public interface IUnit<VType> {
          string Name { get; }
          bool CanConvertFrom(IUnit<VType> other);
         VType ConvertFrom (IUnit<VType> other, VType orgValue) ;
    }
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

    public partial class Service : BaseProduct
    {
        public string ContextId { get; set; }
        [ForeignKey("ContextId")]
        public virtual Activity Context { get; set; }

        public BillingMode? Billing { get; set; }
        // TODO public ServiceSpecification Specification { get; set; }
        /// <summary>
        /// In euro
        /// </summary>
        /// <returns></returns>
        public decimal? Pricing { get; set; }
        
    }

}