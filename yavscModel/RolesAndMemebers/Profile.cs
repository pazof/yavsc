using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Profile;
using System.Web.Security;
using System.Web;

namespace Yavsc.Model.RolesAndMembers
{
	public class Profile
	{
		[DisplayName ("Nom complet")]
		[StringLength (1024)]
		public string Name { get; set; }

		[DisplayName("Avatar")]
		public string avatar { get; set; }

		[DisplayName ("Adresse")]
		[StringLength (2047)]
		public string Address { get; set; }

		[DisplayName ("Ville")]
		[StringLength (255)]
		public string CityAndState { get; set; }

		[DisplayName ("Code Postal")]
		[StringLength (9)]
		public string ZipCode { get; set; }

		[DisplayName ("Pays")]
		[StringLength (99)]
		public string Country { get; set; }

		[DisplayName ("Site Web")]
		[StringLength (255)]
		public string WebSite { get; set; }

		[DisplayName ("Blog visible")]
		public bool BlogVisible { get; set; }

		[DisplayName ("Titre du blog")]
		[StringLength (255)]
		public string BlogTitle { get; set; }

		[DisplayName ("Téléphone fixe")]
		[StringLength (15)]
		public string Phone { get; set; }

		[DisplayName ("Portable")]
		[StringLength (15)]
		public string Mobile { get; set; }

		[DisplayName ("E-mail")]
		[StringLength (1024)]
		public string Email { get; set; }

		[DisplayName ("Code BIC")]
		[StringLength (15)]
		public string BIC { get; set; }

		[DisplayName ("Code IBAN")]
		[StringLength (33)]
		public string IBAN { get; set; }


		[DisplayName ("Code Banque")]
		[StringLength (5)]
		public string BankCode { get; set; }

		[DisplayName ("Code Guichet")]
		[StringLength (5)]
		public string WicketCode { get; set; }

		[DisplayName ("Numéro de compte")]
		[StringLength (15)]
		public string AccountNumber { get; set; }

		[DisplayName ("Clé RIB")]
		public int BankedKey { get; set; }

		[Display(Name="Google_calendar",ResourceType=typeof(LocalizedText))]
		public string GoogleCalendar { get; set; }

		public bool HasBankAccount { get { 
				return IsBillable 
			&& !string.IsNullOrWhiteSpace (BankCode)
			&& !string.IsNullOrWhiteSpace (BIC)
			&& !string.IsNullOrWhiteSpace (IBAN)
			&& !string.IsNullOrWhiteSpace (WicketCode)
			&& !string.IsNullOrWhiteSpace (AccountNumber)
			&& BankedKey != 0; } }

		public bool IsBillable { 
			get { 
				return !string.IsNullOrWhiteSpace (Name)
				&& !string.IsNullOrWhiteSpace (Address)
				&& !string.IsNullOrWhiteSpace (CityAndState)
				&& !string.IsNullOrWhiteSpace (ZipCode)
				&& !string.IsNullOrWhiteSpace (Email)
				&& !(string.IsNullOrWhiteSpace (Phone) &&
				string.IsNullOrWhiteSpace (Mobile)); 
			} 
		}

		public Profile () : base ()
		{
		}

		public bool RememberMe { get; set; }

		public Profile (ProfileBase profile)
		{
			object b = profile.GetPropertyValue ("BlogVisible");
			BlogVisible = (b == null) ? true : (b is DBNull)? true : (bool)b;

			object s = profile.GetPropertyValue ("BlogTitle");
			BlogTitle = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("avatar");
			avatar = (s is DBNull) ? null : (string)s;
	
			s = profile.GetPropertyValue ("Address");
			Address = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("CityAndState");
			CityAndState = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("Country");
			Country = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("ZipCode");
			ZipCode = (s is DBNull) ? null : (string)s;
		
			s = profile.GetPropertyValue ("WebSite");
			WebSite = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("Name");
			Name = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("Phone");
			Phone = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("Mobile");
			Mobile = (s is DBNull) ? null : (string)s;

			MembershipUser u = Membership.GetUser (profile.UserName);
			Email = u.Email;

			s = profile.GetPropertyValue ("BankCode");
			BankCode = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("IBAN");
			IBAN = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("BIC");
			BIC = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("WicketCode");
			WicketCode = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("AccountNumber");
			this.AccountNumber = (s is DBNull) ? null : (string)s;

			s = profile.GetPropertyValue ("BankedKey");
			BankedKey = (s == null) ? 0 : (s is DBNull)? 0 : (int)s;

			s = profile.GetPropertyValue ("gcalid");
			GoogleCalendar = (s is DBNull)? null : (string) s;
		}
	}
}

