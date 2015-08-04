//
//  TexToPdfFormatter.cs
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
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Diagnostics;
using System.Net.Http;
using Yavsc.Helpers;

namespace Yavsc.Formatters
{
	/// <summary>
	/// Tex to pdf formatter.
	/// </summary>
	public class TexToPdfFormatter: BufferedMediaTypeFormatter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Formatters.TexToPdfFormatter"/> class.
		/// </summary>
		public TexToPdfFormatter ()
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/pdf"));
		}
		/// <summary>
		/// Determines whether this instance can read type the specified type.
		/// </summary>
		/// <returns><c>true</c> if this instance can read type the specified type; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		public override bool CanReadType(Type type)
		{
			return false;
		}
		/// <summary>
		/// Determines whether this instance can write type the specified type.
		/// </summary>
		/// <returns><c>true</c> if this instance can write type the specified type; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		public override bool CanWriteType(System.Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			else
			{
				Type enumerableType = typeof(IEnumerable<string>);
				return enumerableType.IsAssignableFrom(type);
			}
		}
		/// <summary>
		/// Writes to stream.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="value">Value.</param>
		/// <param name="stream">Stream.</param>
		/// <param name="contentHeaders">Content headers.</param>
		public override void WriteToStream (Type type, object value, Stream stream, HttpContentHeaders contentHeaders)
		{
			string temp = Path.GetTempPath ();
			string cntStr = value as string;
			string name = "tmpdoc-"+Guid.NewGuid().ToString();
			string fullname = Path.Combine (temp, name);
			FileInfo fi = new FileInfo(fullname + ".tex");
			FileInfo fo = null;
			using (StreamWriter sw = new StreamWriter (fi.OpenWrite()))
			{
				sw.Write (cntStr);
				sw.Close ();
			}

			using (Process p = new Process ()) {
				
				Directory.SetCurrentDirectory (temp);

				p.StartInfo.WorkingDirectory = temp;
				p.StartInfo = new ProcessStartInfo ();
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.FileName = "texi2pdf";
				p.StartInfo.Arguments = 
					string.Format ("--batch {0}",
						fi.FullName);
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;

				p.Start ();
				p.WaitForExit ();

				if (p.ExitCode != 0) {
					var ex = new FormatterException ("Pdf generation failed with exit code:" + p.ExitCode);
					ex.Output = p.StandardOutput.ReadToEnd ()+"\nCWD:"+temp;
					ex.Error = p.StandardError.ReadToEnd ();
					throw ex;
				}
				fo = new FileInfo(name + ".pdf");
			}

			byte[] buffer = File.ReadAllBytes (fo.Name);
			stream.Write(buffer,0,buffer.Length);
			if (contentHeaders != null)
				SetFileName(contentHeaders, value.GetHashCode ().ToString ());
		}

		/// <summary>
		/// Sets the name of the file.
		/// </summary>
		/// <param name="contentHeaders">Content headers.</param>
		/// <param name="basename">Basename.</param>
		public static void SetFileName(HttpContentHeaders contentHeaders, string basename) {
			contentHeaders.ContentDisposition = new ContentDispositionHeaderValue ("attachment") {
				FileName = "doc-" + basename + ".pdf"
			};
		}
	}
}
