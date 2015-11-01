//
//  ValidateAjaxAttribute.cs
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

namespace Yavsc
{
	/// <summary>
	/// Validate ajax attribute.
	/// </summary>
	public class ValidateAjaxAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// Gets the error model object.
		/// </summary>
		/// <returns>The error model object.</returns>
		/// <param name="modelState">Model state.</param>
		public static object GetErrorModelObject(ModelStateDictionary modelState) {
			var errorModel = 
				from x in modelState.Keys
					where modelState[x].Errors.Count > 0
				select new
			{
				// FIXME why not directly underscores?
				key = x.Replace(".","_"),
				errors = modelState[x].Errors.
					Select(y => y.ErrorMessage).
					ToArray()
			};
			return errorModel;

		}
		/// <summary>
		/// Raises the action executed event.
		/// </summary>
		/// <param name="actionExecutedContext">Action executed context.</param>
		public override void OnActionExecuted (HttpActionExecutedContext actionExecutedContext)
		{
			var modelState = actionExecutedContext.ActionContext.ModelState;

			if (!modelState.IsValid)
			{
				actionExecutedContext.Response =
					actionExecutedContext.Request.CreateResponse (System.Net.HttpStatusCode.BadRequest,
						ValidateAjaxAttribute.GetErrorModelObject (modelState));
			}
		}
	}
}

