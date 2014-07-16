using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Profile;

namespace yavscModel.RolesAndMembers
{
	public class Profile
	{
		[DisplayName("Adresse")]
		[StringLength(2047)]
		public string Address { get; set; }

		[DisplayName("Ville")]
		[StringLength(255)]
		public string CityAndState { get; set; }

		[DisplayName("Code Postal")]
		[StringLength(9)]
		public string ZipCode { get; set; }

		[DisplayName("Pays")]
		[StringLength(99)]
		public string Country { get; set; }

		[DisplayName("Site Web")]
		[StringLength(255)]
		public string WebSite { get; set; }

		[DisplayName("Blog visible")]
		public bool BlogVisible { get; set; }

		[DisplayName("Titre du blog")]
		public string BlogTitle { get; set; }
	
		public void FromProfileBase(ProfileBase profile)
		{
				object b = profile.GetPropertyValue ("BlogVisible");
				BlogVisible = (b is DBNull) ? true : (bool)b;

				object s = profile.GetPropertyValue ("BlogTitle");
				BlogTitle = (s is DBNull) ? null : (string)s;
		
				 s = profile.GetPropertyValue("Address");
				Address = (s is DBNull)?null:(string)s;

				 s =  profile.GetPropertyValue("CityAndState");
				CityAndState = (s is DBNull)?null:(string)s;

				 s =  profile.GetPropertyValue("Country");
				Country = (s is DBNull)?null:(string)s;

				 s = profile.GetPropertyValue("ZipCode");
				ZipCode = (s is DBNull)?null:(string)s;

			
				 s = profile.GetPropertyValue ("WebSite");
				WebSite = (s is DBNull) ? null : (string)s;
			
		}
	}
}

