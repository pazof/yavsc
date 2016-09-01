#!/bin/sh

dnx gen controller -outDir ApiControllers -api -dc ApplicationDbContext -m "$1" -name "$2ApiController"

# dnx gen controller -outDir Controllers -dc ApplicationDbContext -udl -m {model} -name {name}Controller


