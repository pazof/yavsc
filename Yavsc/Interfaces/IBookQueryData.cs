using System;
using Yavsc.Model;

namespace Yavsc.Interfaces
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