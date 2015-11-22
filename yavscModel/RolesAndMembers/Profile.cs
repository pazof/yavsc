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
		/// Gets or sets the user interface theme.
		/// </summary>
		/// <value>The user interface theme.</value>
		[DisplayName ("Thème")]
		[StringLength (2047)]
		public string UITheme { get; set; }
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
		/// Gets a value indicating whether this instance has postal address.
		/// </summary>
		/// <value><c>true</c> if this instance has postal address; otherwise, <c>false</c>.</value>
		public bool HasPostalAddress {
			get { 
				return !string.IsNullOrWhiteSpace (Address)
				&& !string.IsNullOrWhiteSpace (CityAndState)
				&& !string.IsNullOrWhiteSpace (ZipCode);
			}
		}
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
				// true if has a name and, either a postal address, an email, or a Mobile phone number
				// Name is not null and 
				// (
				//  (Address and CityAndState and ZipCode)
				//  or Email or Phone or Mobile
				// )
				return !string.IsNullOrWhiteSpace (Name)
				&& !( (string.IsNullOrWhiteSpace (Address)
						|| string.IsNullOrWhiteSpace (CityAndState)
						|| string.IsNullOrWhiteSpace (ZipCode))
				&& string.IsNullOrWhiteSpace (Phone) 
				&& string.IsNullOrWhiteSpace (Mobile)); 
			} 
		}
		private string userName;

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[Localizable(true), Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur valide")
			,Display(ResourceType=typeof(LocalizedText),Name="User_name"),
			RegularExpression("([a-z]|[A-Z]|[\\s-_.~]|[0-9])+")
		]
		public string UserName { get { return userName; } set { userName=value; } } 

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
			if (profile == null) throw new Exception ("No profile");
			if (profile.UserName == null) throw new Exception ("UserName not set");
			UITheme = (string) profile.GetPropertyValue ("UITheme");
			userName = profile.UserName;
			if (profile.IsAnonymous) return;
			BlogVisible = (bool) profile.GetPropertyValue ("BlogVisible");
			BlogTitle = (string) profile.GetPropertyValue ("BlogTitle");
			avatar = (string) profile.GetPropertyValue ("Avatar");
			Address = (string) profile.GetPropertyValue ("Address"); 
			CityAndState = (string) profile.GetPropertyValue ("CityAndState");
			Country = (string) profile.GetPropertyValue ("Country");
			ZipCode = (string) profile.GetPropertyValue ("ZipCode");
			WebSite = (string) profile.GetPropertyValue ("WebSite");
			Name = (string) profile.GetPropertyValue ("Name");
			Phone = (string) profile.GetPropertyValue ("Phone");
			Mobile = (string) profile.GetPropertyValue ("Mobile");
			BankCode = (string)profile.GetPropertyValue ("BankCode");
			IBAN = (string)profile.GetPropertyValue ("IBAN");
			BIC =  (string)profile.GetPropertyValue ("BIC");
			WicketCode = (string) profile.GetPropertyValue ("WicketCode");
			AccountNumber = (string) profile.GetPropertyValue ("AccountNumber");
			BankedKey = (int)  profile.GetPropertyValue ("BankedKey");;
			GoogleCalendar = (string) profile.GetPropertyValue ("gcalid");
		}
	}
}

