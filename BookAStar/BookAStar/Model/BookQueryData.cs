﻿using BookAStar.Interfaces;
using BookAStar.Model.Social;
using System;

namespace BookAStar.Model
{
    public class BookQueryData : IBookQueryData
    {
        public ClientProviderInfo Client { get; set; }
        public Location Location { get; set; }
        public long Id { get; set; }
        public DateTime EventDate { get; set; }
        public decimal? Previsionnal { get; set; }
    }
}
