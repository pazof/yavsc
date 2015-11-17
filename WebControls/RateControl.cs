//
//  RateControl.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
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
using Yavsc.Model;

namespace Yavsc
{
	/// <summary>
	/// Rate control.
	/// </summary>
	public class RateControl<TModel> : ViewUserControl<TModel> where TModel : IRating
	{
		/// <summary>
		/// Initializes a new instance of the Yavsc.Blogs.RateControl class.
		/// </summary>
		public RateControl ()
		{
		}
	
		/// <summary>
		/// Gets or sets the rate, that is, an integer between 0 and 100
		/// </summary>
		/// <value>The rate.</value>
		public int Rate 
		{
			get { return (int) ViewState["rate"]; } 
			set { 
				ViewState["rate"] = value; 
				int rate = value;
				int rounded = (rate / 10);
				HasHalf = rounded % 2 == 1;
				NbFilled = (int)rounded / 2;
				NbEmpty = (5 - NbFilled) - ((HasHalf)?1:0) ;
			}
		}

		/// <summary>
		/// Gets the nb filed.
		/// </summary>
		/// <value>The nb filed.</value>
		public int NbFilled {
			set { ViewState["nbfilled"] = value; } 
			get { return (int) ViewState["nbfilled"]; }
		} 

		/// <summary>
		/// Gets the nb empty.
		/// </summary>
		/// <value>The nb empty.</value>
		public int NbEmpty {
			set { ViewState["nbempty"] = value; } 
			get { return (int) ViewState["nbempty"]; }
		} 

		/// <summary>
		/// Gets a value indicating whether this instance has half.
		/// </summary>
		/// <value><c>true</c> if this instance has half; otherwise, <c>false</c>.</value>
		public bool HasHalf {
			set { ViewState["hashalf"] = value; } 
			get { return (bool) ViewState["hashalf"]; }
		} 

		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			Rate = this.Model.Rate;
		}
	}
}

