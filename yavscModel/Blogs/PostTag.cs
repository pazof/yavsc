//
//  PostTag.cs
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
using System.Configuration;
using System.Collections.Generic;
using Yavsc.Model.Blogs;
using System.Linq;
using Yavsc.Model.Circles;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{

	public class PostTag {
		public long PostId { get; set; }

		[StringLength(512)]
		[RegularExpression(@"^[a-zA-Z0-9 ]+$",ErrorMessage = "Un tag n'est composé que de lettres et de chiffres, les espaces" +
			"sont autorisés")]
		[Required(ErrorMessage = "S'il vous plait, saisissez nom de tag")]
		public string Tag { get; set; }

	}
	
}
