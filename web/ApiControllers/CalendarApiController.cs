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
using Yavsc.ApiControllers.Calendar.Model;
using Yavsc.Model.RolesAndMembers;
using System.Web.Security;
using Yavsc.Model.Google;
using Yavsc.Helpers;
using System.Web.Profile;
using Yavsc.Model.Circles;


namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Night flash controller.
	/// </summary>
	public class CalendarApiController: ApiController
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


