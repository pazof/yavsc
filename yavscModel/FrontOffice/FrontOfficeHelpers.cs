//
//  FrontOfficeHelpers.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
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
using Yavsc.Client.FrontOffice;
using System.Collections.Generic;
using System.Reflection;

namespace Yavsc.Model.FrontOffice
{
	public static class FrontOfficeHelpers
	{
		public static void SetParameters(this Command ycmd, Dictionary<string,string> collection)
		{
			foreach (string key in collection.Keys) {
				if (key != "productref" && key != "type" && key != "clientname" ) {
					ycmd.Parameters.Add (key, collection [key]);
					foreach (var prop in ycmd.GetType().GetRuntimeProperties())
					{
						if (prop.Name == key && prop.CanWrite) {
							System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(prop.PropertyType);
							prop.SetValue(ycmd,tc.ConvertFromString(collection [key]));
						}
					}
				}
			}
		}
		public static Command CreateCommand(string cls) { 
			Type type = Type.GetType(cls);

			if (type == null)
				throw new InvalidOperationException (
					"Cannot find the command class " + cls);

			if (!typeof(Command).IsAssignableFrom (type))
				throw new InvalidOperationException (
					"No command is assignable from a " + cls);

			ConstructorInfo ci = type.GetConstructor (new Type[]{ });
			Yavsc.Client.FrontOffice.NominativeSimpleBookingQuery res;
			var result =  ci.Invoke (new object[]{ }) as Command;
			return result;
		}
	}
}

