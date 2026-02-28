// Copyright (C) 2016  Paul Schneider
//
// This file is part of yavsc.
//
// yavsc is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
//
// yavsc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with yavsc.  If not, see <http://www.gnu.org/licenses/>.
//

using System;

namespace Yavsc
{
    public interface IMobileDeviceDeclaration
    {
        string DeviceId { get; set; }
        string Model { get; set; }
        string Platform { get; set; }
        string Version { get; set; }
        DateTime? LatestActivityUpdate { get; set; }
        string DeviceOwnerId { get; set; }
    }

}
