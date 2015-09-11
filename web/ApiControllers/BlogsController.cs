using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Npgsql.Web.Blog;
using Yavsc.Model.Blogs;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Blogs API controller.
	/// </summary>
	public class BlogsController : YavscApiController
	{
		private const string adminRoleName = "Admin";

		/// <summary>
		/// Initialize the specified controllerContext.
		/// </summary>
		/// <param name="controllerContext">Controller context.</param>
		protected override void Initialize (System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize (controllerContext);
			if (!Roles.RoleExists (adminRoleName)) {
				Roles.CreateRole (adminRoleName);
			}
		}

		/// <summary>
		/// Tag the specified postid and tag.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tag">Tag.</param>
		public long Tag (long postid,string tag) {
			BlogManager.GetForEditing (postid);
			return BlogManager.Tag (postid, tag);
		}

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		[Authorize]
		public void RemoveTitle(string user, string title) {
			if (Membership.GetUser ().UserName != user)
			if (!Roles.IsUserInRole("Admin"))
				throw new AuthorizationDenied (user);
			BlogManager.RemoveTitle (user, title);
		}
		/// <summary>
		/// Removes the tag.
		/// </summary>
		/// <param name="tagid">Tagid.</param>
		public void RemoveTag(long tagid) {
			
			throw new NotImplementedException ();
		}
		/// <summary>
		/// The allowed media types.
		/// </summary>
		protected string[] allowedMediaTypes = { 
			"text/plain",
			"text/x-tex",
			"text/html",
			"image/png",
			"image/gif",
			"image/jpeg",
			"image/x-xcf",
			"application/pdf",
			"application/vnd.openxmlformats-officedocument.wordprocessingml.document"
		};

		/// <summary>
		/// Posts the file.
		/// </summary>
		/// <returns>The file.</returns>
		[Authorize]
		public async Task<HttpResponseMessage> PostFile(long postid) {
			if (!(Request.Content.Headers.ContentType.MediaType=="multipart/form-data"))
			{ 
				throw new HttpRequestException ("not a multipart/form-data request");
			}

			string root = HttpContext.Current.Server.MapPath("~/bfiles/"+postid);
			BlogEntry be = BlogManager.GetPost (postid);
			if (be.UserName != Membership.GetUser ().UserName)
				throw new AuthorizationDenied ("b"+postid);
			
			DirectoryInfo di = new DirectoryInfo (root);
			if (!di.Exists) di.Create ();
			
			var provider = new MultipartFormDataStreamProvider(root);
			try
			{


				// Read the form data.
				foreach (var content in await Request.Content.ReadAsMultipartAsync(provider)) {
					Trace.WriteLine("Server file path: " + provider.GetLocalFileName(
						content.Headers));
				}

				// This illustrates how to get the file names.
				foreach (string fkey in provider.BodyPartFileNames.Keys)
				{
					Trace.WriteLine(provider.BodyPartFileNames[fkey]);

				}

				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (System.Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
			}
		}
			
	}
}

