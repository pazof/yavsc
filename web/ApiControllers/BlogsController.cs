﻿using System;
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
using Yavsc.Formatters;
using Yavsc.Model;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Blogs API controller.
	/// </summary>
	public class BlogsController : YavscController
	{
		/// <summary>
		/// Tag the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize, 
			AcceptVerbs ("POST")]
		public void Tag (PostTag model) {
			if (ModelState.IsValid) {
				BlogManager.GetForEditing (model.PostId);
				BlogManager.Tag (model.PostId, model.Tag);
			}
		}
		static string [] officalTags = new string[] { "Artistes", "Accueil", "Événements", "Mentions légales", "Admin", "Web" } ;
		/// <summary> 
		/// Tags the specified pattern.
		/// </summary>
		/// <param name="pattern">Pattern.</param>
		[ValidateAjaxAttribute]
		public IEnumerable<string> Tags(string pattern)
		{
			return officalTags;
		}

		/// <summary>
		/// Untag the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize, 
			AcceptVerbs ("POST")]
		public void Untag (PostTag model) {
			if (ModelState.IsValid) {
				BlogManager.GetForEditing (model.PostId);
				BlogManager.Untag (model.PostId, model.Tag);
			}
		}

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		[Authorize, ValidateAjaxAttribute, HttpPost]
		public void RemoveTitle(string user,  string title) {
			if (Membership.GetUser ().UserName != user)
			if (!Roles.IsUserInRole("Admin"))
				throw new AuthorizationDenied ();
			BlogManager.RemoveTitle (user, title);
		}

		/// <summary>
		/// Removes the tag.
		/// </summary>
		/// <param name="tagid">Tagid.</param>
		[Authorize, ValidateAjaxAttribute, HttpPost]
		public void RemoveTag([FromBody] long tagid) {
			
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
		[Authorize, HttpPost]
		public async Task<HttpResponseMessage> PostFile(long id) {
			if (!(Request.Content.Headers.ContentType.MediaType=="multipart/form-data"))
				throw new HttpRequestException ("not a multipart/form-data request");
			if (id == 0) {
				throw new NotImplementedException ();
			}

			BlogEntry be = BlogManager.GetPost (id);
			if (be.Author != Membership.GetUser ().UserName)
				throw new AuthorizationDenied ();
			string root = HttpContext.Current.Server.MapPath("~/bfiles/"+id);
			DirectoryInfo di = new DirectoryInfo (root);
			if (!di.Exists) di.Create ();

			var provider = new MultipartFormDataStreamProvider(root);
			try
			{
				// Read the form data.
				await Request.Content.ReadAsMultipartAsync(provider) ;
				var invalidChars = Path.GetInvalidFileNameChars();
				foreach (var file in provider.FileData)
				{
					string filename = file.LocalFileName;
					Trace.WriteLine(filename);
					string nicename = file.Headers.ContentDisposition.FileName ;
					nicename = new string (nicename.Where( x=> !invalidChars.Contains(x)).ToArray());
					nicename = nicename.Replace(' ','_');
					var dest = Path.Combine(root,nicename);
					var fi = new FileInfo(dest);
					if (fi.Exists) fi.Delete();
					File.Move(filename, fi.FullName);
				}

				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (System.Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
			}
		}

		/// <summary>
		/// Create the specified blog entry.
		/// </summary>
		/// <param name="be">Bp.</param>
		[Authorize, HttpPost]
		public long Post (BlogEntry be) 
		{
			if (be.Id == 0)
				return BlogManager.Post (User.Identity.Name, be.Title, 
					be.Content, be.Visible,be.AllowedCircles );
			else
				BlogManager.UpdatePost (be);
			return 0;
		}
		/// <summary>
		/// Blog entry rating.
		/// </summary>
		public class BlogEntryRating { 
			/// <summary>
			/// Gets or sets the post identifier.
			/// </summary>
			/// <value>The post identifier.</value>
			public long Id { get; set; }
			/// <summary>
			/// Gets or sets the rate.
			/// </summary>
			/// <value>The rate.</value>
			public int Rate { get; set; }
		}

		/// <summary>
		/// Rate the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize(Roles="Moderator"), HttpPost, ValidateAjax]
		public void Rate (BlogEntryRating model)
		{
			if (model.Rate < 0 || model.Rate > 100)
				ModelState.AddModelError ("Rate", "0<=Rate<=100");
			else {
				BlogManager.GetForEditing (model.Id);
				BlogManager.Rate (model.Id, model.Rate);
			}
		}
		/// <summary>
		/// Searchs the file.
		/// </summary>
		/// <returns>The file.</returns>
		/// <param name="id">Postid.</param>
		/// <param name="terms">Terms.</param>
		[HttpGet]
		public async Task<HttpResponseMessage> SearchFile(long id, string terms) {
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Sets the photo.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="photo">Photo.</param>
		[Authorize, HttpPost, ValidateAjaxAttribute]
		public void SetPhoto(long id, [FromBody] string photo)
		{
			BlogManager.GetForEditing (id);
			BlogManager.UpdatePostPhoto (id, photo);
		}

		/// <summary>
		/// Import the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize, HttpPost, ValidateAjaxAttribute]
		public async Task<HttpResponseMessage> Import(long id) {
			if (!(Request.Content.Headers.ContentType.MediaType=="multipart/form-data"))
				throw new HttpRequestException ("not a multipart/form-data request");
			BlogEntry be = BlogManager.GetPost (id);
			if (be.Author != Membership.GetUser ().UserName)
				throw new AuthorizationDenied ();
			string root = HttpContext.Current.Server.MapPath("~/bfiles/"+id);
			DirectoryInfo di = new DirectoryInfo (root);
			if (!di.Exists) di.Create ();
			var provider = new MultipartFormDataStreamProvider(root);
			try
			{
				// Read the form data.
				//IEnumerable<HttpContent> data = 
				await Request.Content.ReadAsMultipartAsync(provider) ;

				var invalidChars = Path.GetInvalidFileNameChars();
				List<string> bodies = new List<string>();
				 
				foreach (var file in provider.FileData)
				{
					string filename = file.LocalFileName;
					
					string nicename= file.Headers.ContentDisposition.FileName;
					var filtered  = new string (nicename.Where( x=> !invalidChars.Contains(x)).ToArray());

					FileInfo fi = new FileInfo(filtered);
					FileInfo fo = new FileInfo(filtered+".md");
					FileInfo fp = new FileInfo (Path.Combine(root,filename));
					if (fi.Exists) fi.Delete();
					fp.MoveTo(fi.FullName);
					// TODO Get the mime type
					using (Process p = new Process ()) {			
						p.StartInfo.WorkingDirectory = root;
						p.StartInfo = new ProcessStartInfo ();
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.FileName = "/usr/bin/pandoc";
						p.StartInfo.Arguments = 
							string.Format (" -o '{0}' -t markdown '{1}'",
								fo.FullName,
								fi.FullName);
						p.StartInfo.RedirectStandardError = true;
						p.StartInfo.RedirectStandardOutput = true;
						p.Start ();
						p.WaitForExit ();
						if (p.ExitCode != 0) { 
							return Request.CreateResponse (HttpStatusCode.InternalServerError,
								"# Import failed with exit code: " + p.ExitCode + "---\n" 
								+  LocalizedText.ImportException + "---\n" 
										+ p.StandardError.ReadToEnd() + "---\n" 
										+ p.StandardOutput.ReadToEnd()
									);
						}
					}
					bodies.Add(fo.OpenText().ReadToEnd());

					 
					fi.Delete();
					fo.Delete();
				}

				return Request.CreateResponse(HttpStatusCode.OK,string.Join("\n---\n",bodies),new SimpleFormatter("text/plain"));

			}
			catch (System.Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
			}
		}
	}
}

