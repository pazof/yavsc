using System;
using BookAStar.Model.Social;
using BookAStar.Model;

namespace BookAStar.Interfaces
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