using System;
using ZicMoove.Model.Social;
using ZicMoove.Model;

namespace ZicMoove.Interfaces
{
    public interface IBookQueryData
    {
        ClientProviderInfo Client { get; set; }
        DateTime EventDate { get; set; }
        long Id { get; set; }
        Location Location { get; set; }
        decimal? Previsionnal { get; set; }
    }
}