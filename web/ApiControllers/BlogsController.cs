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
using Yavsc.Formatters;
using Yavsc.Model;

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
		[Authorize,HttpPost]
		public async Task<HttpResponseMessage> PostFile(long id) {
			if (!(Request.Content.Headers.ContentType.MediaType=="multipart/form-data"))
				throw new HttpRequestException ("not a multipart/form-data request");
			BlogEntry be = BlogManager.GetPost (id);
			if (be.Author != Membership.GetUser ().UserName)
				throw new AuthorizationDenied ("b"+id);
			string root = HttpContext.Current.Server.MapPath("~/bfiles/"+id);
			DirectoryInfo di = new DirectoryInfo (root);
			if (!di.Exists) di.Create ();
			var provider = new MultipartFormDataStreamProvider(root);
			try
			{
				// Read the form data.
				await Request.Content.ReadAsMultipartAsync(provider) ;
				var invalidChars = Path.GetInvalidFileNameChars();
				foreach (string fkey in provider.BodyPartFileNames.Keys)
				{
					string filename = provider.BodyPartFileNames[fkey];
					Trace.WriteLine(filename);

					string nicename=fkey;
					if (fkey.StartsWith("\"") && fkey.EndsWith("\"") && fkey.Length > 2)
						nicename = fkey.Substring(1,fkey.Length-2);
					var filtered  = new string (nicename.Where( x=> !invalidChars.Contains(x)).ToArray());
					File.Move(Path.Combine(root,filename),
						Path.Combine(root,filtered));
				}

				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (System.Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
			}
		}
		/// <summary>
		/// Import the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public async Task<HttpResponseMessage> Import(long id) {
			if (!(Request.Content.Headers.ContentType.MediaType=="multipart/form-data"))
				throw new HttpRequestException ("not a multipart/form-data request");
			BlogEntry be = BlogManager.GetPost (id);
			if (be.Author != Membership.GetUser ().UserName)
				throw new AuthorizationDenied ("b"+id);
			string root = HttpContext.Current.Server.MapPath("~/bfiles/"+id);
			DirectoryInfo di = new DirectoryInfo (root);
			if (!di.Exists) di.Create ();
			var provider = new MultipartFormDataStreamProvider(root);
			try
			{
				// Read the form data.
				await Request.Content.ReadAsMultipartAsync(provider) ;

				var invalidChars = Path.GetInvalidFileNameChars();
				List<string> bodies = new List<string>();

				foreach (string fkey in provider.BodyPartFileNames.Keys)
				{
					string filename = provider.BodyPartFileNames[fkey];

					string nicename=fkey;
					if (fkey.StartsWith("\"") && fkey.EndsWith("\"") && fkey.Length > 2)
						nicename = fkey.Substring(1,fkey.Length-2);
					var filtered  = new string (nicename.Where( x=> !invalidChars.Contains(x)).ToArray());

					FileInfo fi = new FileInfo(filtered);
					FileInfo fo = new FileInfo(filtered+".md");
					FileInfo fp = new FileInfo (Path.Combine(root,filename));
					if (fi.Exists) fi.Delete();
					fp.MoveTo(fi.FullName);
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

				return Request.CreateResponse(HttpStatusCode.OK,string.Join("---\n",bodies),new SimpleFormatter("text/plain"));

			}
			catch (System.Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
			}
		}
	}
}

