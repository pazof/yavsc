

namespace Yavsc.Models.Market
{
    public partial class Product : BaseProduct
    {
        /// <summary>
        /// Weight in gram
        /// </summary>
        /// <returns></returns>
        public decimal Weight { get; set; }
        /// <summary>
        /// Height in meter
        /// </summary>
        /// <returns></returns>
        public decimal Height { get; set; }
        /// <summary>
        /// Width in meter
        /// </summary>
        /// <returns></returns>
        public decimal Width { get; set; }
        public decimal Depth { get; set; }
        /// <summary>
        /// In euro
        /// </summary>
        /// <returns></returns>
        public decimal? Price { get; set; }
    }

}
