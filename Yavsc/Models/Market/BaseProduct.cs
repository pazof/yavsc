

namespace Yavsc.Models.Market
{
    public class BaseProduct
    {

        public string Name { get; set; }
        /// <summary>
        /// A contractual description for this product.
        /// </summary>
        /// <returns></returns>
        public string Description { get; set; }

        /// <summary>
        /// Controls wether this product or service
        /// may be offered to clients, or simply
        /// are internal workflow entry point.
        /// </summary>
        /// <returns>true when this product belongs to the public catalog.</returns>
        public bool Public { get; set; }
    }

}
