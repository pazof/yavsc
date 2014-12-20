﻿//
//  ModuleConfigurationElement.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2014 Paul Schneider
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Configuration;

namespace Yavsc.Settings
{
	public class ModuleConfigurationElement : ConfigurationElement
	{
		public ModuleConfigurationElement ()
		{
		}

		[ConfigurationProperty("name", IsKey=true, IsRequired=true)]
		public string Name {
			get {
				return (string) base ["name"];
			}
			set { base ["name"] = value; }
		}

		[ConfigurationProperty("name", IsKey=true, IsRequired=true)]
		public string ClassName {
			get {
				return (string) base ["classname"];
			}
			set { base ["classname"] = value; }
		}

	}
}

