using System;
using Yavsc.Abstract.Identity;

namespace Yavsc.Interfaces
{
    public interface IBookQueryData
    {
        ClientProviderInfo Client { get; set; }
        DateTime EventDate { get; set; }
        long Id { get; set; }
        ILocation Location { get; set; }
        decimal? Previsionnal { get; set; }
    }
}