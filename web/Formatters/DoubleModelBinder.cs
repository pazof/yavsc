//
//  DecimalModelBinder.cs
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
using System.Web.Mvc;
using System.Threading;
using System.Globalization;
using Yavsc.Model;
using Yavsc.Client;

namespace Yavsc.Formatters
{
	public class DoubleModelBinder: IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var culture = CultureInfo.InvariantCulture;
			var  style = NumberStyles.AllowDecimalPoint ;
			var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			double parsedValue=0;
			ModelState modelState = new ModelState { Value = valueResult };
			if (!double.TryParse (valueResult.AttemptedValue, style, culture, out parsedValue)) 
				modelState.Errors.Add (
					string.Format(
						LocalizedText.CouldNotConvertVToDouble,
						valueResult.AttemptedValue
					));
			bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
			return parsedValue;
		}
	}
}

