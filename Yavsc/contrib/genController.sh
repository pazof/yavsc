#!/bin/sh

dnx gen controller -async -udl -outDir Controllers -dc ApplicationDbContext -m "$1" -name "$2Controller"

# dnx gen controller -outDir Controllers -dc ApplicationDbContext -udl -m {model} -name {name}Controller


# dnx gen controller -outDir Controllers -dc ApplicationDbContext -udl -m Yavsc.Models.Booking.MusicianSettings -name InstrumentationController -async -scripts
