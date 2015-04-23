using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Profile;
using System.Web.Security;
using System.Web;
using System.Configuration;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// Profile.
	/// </summary>
	public class Profile
	{


		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[DisplayName ("Nom complet")]
		[StringLength (1024)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the avatar.
		/// </summary>
		/// <value>The avatar.</value>
		[DisplayName("Avatar")]
		public string avatar { get; set; }

		/// <summary>
		/// Gets or sets the address.
		/// </summary>
		/// <value>The address.</value>
		[DisplayName ("Adresse")]
		[StringLength (2047)]
		public string Address { get; set; }

		/// <summary>
		/// Gets or sets the state of the city and.
		/// </summary>
		/// <value>The state of the city and.</value>
		[DisplayName ("Ville")]
		[StringLength (255)]
		public string CityAndState { get; set; }

		/// <summary>
		/// Gets or sets the zip code.
		/// </summary>
		/// <value>The zip code.</value>
		[DisplayName ("Code Postal")]
		[StringLength (9)]
		public string ZipCode { get; set; }

		/// <summary>
		/// Gets or sets the country.
		/// </summary>
		/// <value>The country.</value>
		[DisplayName ("Pays")]
		[StringLength (99)]
		public string Country { get; set; }

		/// <summary>
		/// Gets or sets the web site.
		/// </summary>
		/// <value>The web site.</value>
		[DisplayName ("Site Web")]
		[StringLength (255)]
		public string WebSite { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.RolesAndMembers.Profile"/> blog visible.
		/// </summary>
		/// <value><c>true</c> if blog visible; otherwise, <c>false</c>.</value>
		[DisplayName ("Blog visible")]
		public bool BlogVisible { get; set; }

		private string blogTitle;

		/// <summary>
		/// Gets or sets the blog title.
		/// </summary>
		/// <value>The blog title.</value>
		[DisplayName ("Titre du blog")]
		[StringLength (255)]
		public string BlogTitle { get { 
				return string.IsNullOrWhiteSpace(blogTitle)? 
					(string.IsNullOrWhiteSpace(Name)?
						UserName:Name)+"'s blog":blogTitle; }
			set { blogTitle = value; } }

		/// <summary>
		/// Gets or sets the phone number.
		/// </summary>
		/// <value>The phone.</value>
		[DisplayName ("Téléphone fixe")]
		[StringLength (15)]
		public string Phone { get; set; }

		/// <summary>
		/// Gets or sets the mobile.
		/// </summary>
		/// <value>The mobile.</value>
		[DisplayName ("Portable")]
		[StringLength (15)]
		public string Mobile { get; set; }

		/// <summary>
		/// Gets or sets the email.
		/// </summary>
		/// <value>The email.</value>
		[DisplayName ("E-mail")]
		[StringLength (1024)]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the BI.
		/// </summary>
		/// <value>The BI.</value>
		[DisplayName ("Code BIC")]
		[StringLength (15)]
		public string BIC { get; set; }

		/// <summary>
		/// Gets or sets the IBA.
		/// </summary>
		/// <value>The IBA.</value>
		[DisplayName ("Code IBAN")]
		[StringLength (33)]
		public string IBAN { get; set; }


		/// <summary>
		/// Gets or sets the bank code.
		/// </summary>
		/// <value>The bank code.</value>
		[DisplayName ("Code Banque")]
		[StringLength (5)]
		public string BankCode { get; set; }

		/// <summary>
		/// Gets or sets the wicket code.
		/// </summary>
		/// <value>The wicket code.</value>
		[DisplayName ("Code Guichet")]
		[StringLength (5)]
		public string WicketCode { get; set; }

		/// <summary>
		/// Gets or sets the account number.
		/// </summary>
		/// <value>The account number.</value>
		[DisplayName ("Numéro de compte")]
		[StringLength (15)]
		public string AccountNumber { get; set; }

		/// <summary>
		/// Gets or sets the banked key.
		/// </summary>
		/// <value>The banked key.</value>
		[DisplayName ("Clé RIB")]
		public int BankedKey { get; set; }

		/// <summary>
		/// Gets or sets the google calendar.
		/// </summary>
		/// <value>The google calendar.</value>
		[Display(Name="Google_calendar",ResourceType=typeof(LocalizedText))]
		public string GoogleCalendar { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance has bank account.
		/// </summary>
		/// <value><c>true</c> if this instance has bank account; otherwise, <c>false</c>.</value>
		public bool HasBankAccount { 
			get { 
				return !(
					(
			string.IsNullOrWhiteSpace (BankCode)
			|| string.IsNullOrWhiteSpace (WicketCode)
			|| string.IsNullOrWhiteSpace (AccountNumber)
			|| BankedKey == 0
					)
					&&
			( string.IsNullOrWhiteSpace (BIC)
			|| string.IsNullOrWhiteSpace (IBAN))
				); } }

		/// <summary>
		/// Gets a value indicating whether this instance is billable.
		/// Returns true when 
		/// Name is not null and all of 
		///  Address, CityAndState and ZipCode are not null,
		///  or one of Email or Phone or Mobile is not null
		/// 
		/// </summary>
		/// <value><c>true</c> if this instance is billable; otherwise, <c>false</c>.</value>
		public bool IsBillable { 
			get { 
				// true if 
				// Name is not null and 
				// (
				//  (Address and CityAndState and ZipCode)
				//  or Email or Phone or Mobile
				// )
				return !string.IsNullOrWhiteSpace (Name)
				&& !( (
						string.IsNullOrWhiteSpace (Address)
						|| string.IsNullOrWhiteSpace (CityAndState)
						|| string.IsNullOrWhiteSpace (ZipCode))
					&& string.IsNullOrWhiteSpace (Email) 
				&& string.IsNullOrWhiteSpace (Phone) 
				&& string.IsNullOrWhiteSpace (Mobile)); 
			} 
		}

		public string UserName { get ; set; } 

		public Profile () : base ()
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.RolesAndMembers.Profile"/> remember me.
		/// </summary>
		/// <value><c>true</c> if remember me; otherwise, <c>false</c>.</value>
		public bool RememberMe { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.RolesAndMembers.Profile"/> class.
		/// </summary>
		/// <param name="profile">Profile.</param>
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

			UserName = profile.UserName;

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

