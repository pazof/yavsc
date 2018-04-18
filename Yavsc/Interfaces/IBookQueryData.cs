using System;

namespace Yavsc.Interfaces
{
    using Models.Relationship;
    using Models.Messaging;
    public interface IBookQueryData
    {
        ClientProviderInfo Client { get; set; }
        DateTime EventDate { get; set; }
        long Id { get; set; }
        Location Location { get; set; }
        decimal? Previsionnal { get; set; }
    }
}