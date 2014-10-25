//
//  EstimToPdfFormatter.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2014 Paul Schneider
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
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Yavsc.Model.WorkFlow;
using Yavsc.Model.RolesAndMembers;
using System.Web.Profile;
using System.Web;
using System.Diagnostics;

namespace Yavsc.Formatters
{
	public class EstimToPdfFormatter: BufferedMediaTypeFormatter
	{
		public EstimToPdfFormatter ()
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/pdf"));
		}

		public override bool CanReadType(Type type)
		{
			return false;
		}

		public override bool CanWriteType(System.Type type)
		{
			if (type == typeof(Estimate))
			{
				return true;
			}
			else
			{
				Type enumerableType = typeof(IEnumerable<Estimate>);
				return enumerableType.IsAssignableFrom(type);
			}
		}

		public override void WriteToStream (Type type, object value, Stream stream, HttpContentHeaders contentHeaders)
		{
			// TODO create a type containing generation parameters, including a template path, and generate from them

			Yavsc.templates.Estim  tmpe = new Yavsc.templates.Estim();
			tmpe.Session = new Dictionary<string,object>();
			Estimate e = value as Estimate;
			tmpe.Session.Add ("estim", e);

			Profile prpro =  new Profile (ProfileBase.Create (e.Responsible));

			var pbc = ProfileBase.Create (e.Client);
			Profile prcli =  new Profile (pbc);
			if (!prpro.IsBankable)
				throw new Exception ("This provider is not bankable.");
			tmpe.Session.Add ("from", prpro);
			tmpe.Session.Add ("to", prcli);
			tmpe.Init ();

			string content = tmpe.TransformText ();

			string name = string.Format ("tmpestimtex{0}", e.Id);
			string fullname = Path.Combine (
				HttpRuntime.CodegenDir, name);
			FileInfo fi = new FileInfo(fullname + ".tex");
			FileInfo fo = new FileInfo(fullname + ".pdf");
			using (StreamWriter sw = new StreamWriter (fi.FullName))
			{
				sw.Write (content);
			}
			using (Process p = new Process ()) {
				p.StartInfo.WorkingDirectory = HttpRuntime.CodegenDir;
				p.StartInfo = new ProcessStartInfo ();
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.FileName = "/usr/bin/texi2pdf";
				p.StartInfo.Arguments = 
					string.Format ("--batch -o {0} {1}",
						fo.FullName,
						fi.FullName);
				p.Start ();
				p.WaitForExit ();
			}

			using (StreamReader sr = new StreamReader (fo.FullName)) {
				byte[] buffer = File.ReadAllBytes (fo.FullName);
				stream.Write(buffer,0,buffer.Length);
			}
			fi.Delete();
			fo.Delete();
		}

	}
}

