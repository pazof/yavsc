//
//  CalendarController.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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
using System.Web.Http;
using System.ComponentModel.DataAnnotations;
using Yavsc.ApiControllers.Calendar.Model;
using Yavsc.Helpers;
using System.ComponentModel;
using Yavsc.Model;
using System.Web.Security;
using System.Web.Profile;
using System.Web.Http.ModelBinding;

namespace Yavsc.ApiControllers.Calendar
{
	/// <summary>
	/// Night flash controller.
	/// </summary>
	public class CalendarController: ApiController
	{
		YaEvent[] getTestList()
		{
			return new YaEvent[] {
				new YaEvent () {
					Description = "Test Descr",
					Title = "Night club special bubble party",
					Location = new Position () {
						Longitude = 0,
						Latitude = 0 
					}
				},
				new YaEvent () {
					Title = "Test2",
					ImgLocator = "http://bla/im.png",
					Location = new Position () {
						Longitude = 0,
						Latitude = 0 
					}
				},
				new YaEvent () {
					Description = "Test Descr",
					Title = "Night club special bubble party",
					Location = new Position () {
						Longitude = 0,
						Latitude = 0 
					}
				},
				new YaEvent () {
					Title = "Test2",
					ImgLocator = "http://bla/im.png",
					Location = new Position () {
						Longitude = 0,
						Latitude = 0 
					}
				}
			};
		}

		/// <summary>
		/// List events according the specified search arguments.
		/// </summary>
		/// <param name="args">Arguments.</param>
		[ValidateAjaxAttribute]
		[HttpGet]
		public YaEvent[] List ([FromUri] PositionAndKeyphrase args)
		{
			return getTestList();
		}

		/// <summary>
		/// Provider the specified ProviderId.
		/// </summary>
		/// <param name="ProviderId">Provider identifier.</param>
		[HttpGet]
		public ProviderPublicInfo ProviderInfo ([FromUri] string ProviderId)
		{
			return new ProviderPublicInfo () {
				DisplayName = "Yavsc clubing",
				WebPage = "http://yavsc.pschneider.fr/",
				Calendar = new Schedule () {
					Period = Periodicity.ThreeM,
					WeekDays = new OpenDay[] { new OpenDay () { Day = WeekDay.Saturday,
							Start = new TimeSpan(18,00,00),
							End = new TimeSpan(2,00,00)
						} },
					Validity = new Period[] { new Period() {
							Start = new DateTime(2015,5,29),
							End = new DateTime(2015,5,30)} }
				},
				Description = "Yavsc Entertainment Production, Yet another private party",
				LogoImgLocator = "http://yavsc.pschneider.fr/favicon.png",
				Location = new Position () { Longitude = 0, Latitude = 0 },
				LocationType = "Salle des fêtes"

			};
		}

		/// <summary>
		/// Posts the image.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="NFProvId">NF prov identifier.</param>
		public string PostImage([FromUri] string NFProvId)
		{
			return null;
		}

		/// <summary>
		/// Posts the event.
		/// </summary>
		/// <returns>The event identifier.</returns>
		/// <param name="ev">Ev.</param>
		public int PostEvent ([FromBody] ProvidedEvent ev)
		{
			throw new NotImplementedException();

		}
		/// <summary>
		/// Notification.
		/// </summary>
		public class Notification {
			/// <summary>
			/// The title.
			/// </summary>
			public string title;
			/// <summary>
			/// The body.
			/// </summary>
			public string body;
			/// <summary>
			/// The icon.
			/// </summary>
			public string icon;
			/// <summary>
			/// The sound.
			/// </summary>
			public string sound;
			/// <summary>
			/// The tag.
			/// </summary>
			public string tag;
			/// <summary>
			/// The color.
			/// </summary>
			public string color;
			/// <summary>
			/// The click action.
			/// </summary>
			public string click_action;

		}

		// https://gcm-http.googleapis.com/gcm/send
		/// <summary>
		/// Message with payload.
		/// </summary>
		public class MessageWithPayload<T> {
			/// <summary>
			/// To.
			/// </summary>
			public string to;
			/// <summary>
			/// The registration identifiers.
			/// </summary>
			public string [] registration_ids; 
			/// <summary>
			/// The data.
			/// </summary>
			public T[] data ;
			/// <summary>
			/// The notification.
			/// </summary>
			public Notification notification;
			/// <summary>
			/// The collapse key.
			/// </summary>
			public string collapse_key; // in order to collapse ...
			/// <summary>
			/// The priority.
			/// </summary>
			public int priority; // between 0 and 10, 10 is the lowest!
			/// <summary>
			/// The content available.
			/// </summary>
			public bool content_available; 
			/// <summary>
			/// The delay while idle.
			/// </summary>
			public bool delay_while_idle; 
			/// <summary>
			/// The time to live.
			/// </summary>
			public int time_to_live; // seconds
			/// <summary>
			/// The name of the restricted package.
			/// </summary>
			public string restricted_package_name;
			/// <summary>
			/// The dry run.
			/// </summary>
			public bool dry_run;
			/// <summary>
			/// Validate the specified modelState.
			/// </summary>
			/// <param name="modelState">Model state.</param>
			public void Validate(ModelStateDictionary modelState) {
				if (to==null && registration_ids == null) {
					modelState.AddModelError ("to", "One of \"to\" or \"registration_ids\" parameters must be specified");
					modelState.AddModelError ("registration_ids", "*");
				}
				if (notification == null && data == null) {
						modelState.AddModelError ("notification", "At least one of \"notification\" or \"data\" parameters must be specified");
					modelState.AddModelError ("data", "*");
				}
				if (notification != null) {
					if (notification.icon == null)
						modelState.AddModelError ("notification.icon", "please, specify an icon resoure name");
					if (notification.title == null)
						modelState.AddModelError ("notification.title", "please, specify a title");
				}
			}
		}

		/// <summary>
		/// Message with payload response.
		/// </summary>
		public class MessageWithPayloadResponse { 
			/// <summary>
			/// The multicast identifier.
			/// </summary>
			public int multicast_id;
			/// <summary>
			/// The success count.
			/// </summary>
			public int success;
			/// <summary>
			/// The failure count.
			/// </summary>
			public int failure;
			/// <summary>
			/// The canonical identifiers... ?!?
			/// </summary>
			public int canonical_ids;
			/// <summary>
			/// Detailled result.
			/// </summary>
			public class Result {
				/// <summary>
				/// The message identifier.
				/// </summary>
				public string message_id;
				/// <summary>
				/// The registration identifier.
				/// </summary>
				public string registration_id;
				/// <summary>
				/// The error.
				/// </summary>
				public string error;
			}

			/// <summary>
			/// The results.
			/// </summary>
			public Result [] results;
		}

		/// <summary>
		/// GCM register model.
		/// </summary>
		public class GCMRegisterModel {
			/// <summary>
			/// Gets or sets the name of the user.
			/// </summary>
			/// <value>The name of the user.</value>
			[Localizable(true)]
			[Display(ResourceType=typeof(LocalizedText),Name="UserName")]
			[Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur")]
			public string UserName { get; set; }

			/// <summary>
			/// Gets or sets the password.
			/// </summary>
			/// <value>The password.</value>
			[DisplayName("Mot de passe")]
			[Required(ErrorMessage = "S'il vous plait, entez un mot de passe")]
			public string Password { get; set; }

			/// <summary>
			/// Gets or sets the email.
			/// </summary>
			/// <value>The email.</value>
			[DisplayName("Adresse e-mail")]
			[Required(ErrorMessage = "S'il vous plait, entrez un e-mail valide")]
			public string Email { get; set; }

			/// <summary>
			/// Gets or sets the registration identifier against Google Clood Messaging and their info on this application.
			/// </summary>
			/// <value>The registration identifier.</value>
			public string RegistrationId { get; set; }

		}

		/// <summary>
		/// Registers with push notifications enabled.
		/// </summary>
		/// <param name="model">Model.</param>
		[ValidateAjax]
		public void RegisterWithPushNotifications(GCMRegisterModel model)
		{
			if (ModelState.IsValid) {
				MembershipCreateStatus mcs;
				var user = Membership.CreateUser (
					model.UserName,
					model.Password,
					model.Email,
					null,
					null,
					false,
					out mcs);
				switch (mcs) {
				case MembershipCreateStatus.DuplicateEmail:
					ModelState.AddModelError ("Email", "Cette adresse e-mail correspond " +
					"à un compte utilisateur existant");
					break;
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError ("UserName", "Ce nom d'utilisateur est " +
					"déjà enregistré");
					break;
				case MembershipCreateStatus.Success:
					YavscHelpers.SendActivationEmail (user);
					// TODO set registration id
					throw new NotImplementedException ();
				}
			}
		}

		/// <summary>
		/// Sets the registration identifier.
		/// </summary>
		/// <param name="registrationId">Registration identifier.</param>
		[Authorize]
		public void SetRegistrationId(string registrationId)
		{
			// TODO set registration id
			setRegistrationId (Membership.GetUser ().UserName, registrationId);
		}

		private void setRegistrationId(string username, string regid) {
			ProfileBase pr = ProfileBase.Create(username);
			pr.SetPropertyValue ("gregid", regid);
		}

		/// <summary>
		/// Notifies the event.
		/// </summary>
		/// <param name="evpub">Evpub.</param>
		public MessageWithPayloadResponse NotifyEvent(EventPub evpub) {
			SimpleJsonPostMethod<MessageWithPayload<YaEvent>,MessageWithPayloadResponse> r = 
				new SimpleJsonPostMethod<MessageWithPayload<YaEvent>,MessageWithPayloadResponse>(
					"https://gcm-http.googleapis.com/gcm/send");
			using (r) { 
				var msg = new MessageWithPayload<YaEvent> () { data = new YaEvent[] { (YaEvent)evpub } };
				msg.to = string.Join (" ", Circle.Union (evpub.Circles));

				return r.Invoke (msg);
			}
		}
	}
}


