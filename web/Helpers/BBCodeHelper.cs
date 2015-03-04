using System;
using CodeKicker.BBCode;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.Text;

namespace Yavsc.Helpers
{
	/// <summary>
	/// BB code helper.
	/// </summary>
	public static class BBCodeHelper
	{
		static Dictionary<string,BBTag> parent = new Dictionary<string,BBTag>  ();
		private static string tclass="shoh"; 
		private static string cclass="hiduh";
		private static string mp4=null, ogg=null, webm=null;
		/// <summary>
		/// Gets or sets the BB code view class.
		/// </summary>
		/// <value>The BB code view class.</value>
		public static string BBCodeViewClass {
			get {
				return cclass;
			}
			set {
				cclass = value;
			}
		}
		/// <summary>
		/// Gets or sets the BB code case class.
		/// </summary>
		/// <value>The BB code case class.</value>
		public static string BBCodeCaseClass {
			get {
				return tclass;
			}
			set {
				tclass = value;
			}
		}
		/// <summary>
		/// Gets the BB tags usage.
		/// </summary>
		/// <value>The BB tags usage.</value>
		public static string[] BBTagsUsage {
			get {

				List<string> u = new List<string> ();
				foreach (BBTag t in Parser.Tags)
					if (!parent.ContainsValue(t))
				 {
						TagBuilder tib = new TagBuilder ("div");
						tib.AddCssClass (BBCodeCaseClass);
						string temp = tagUsage (t);

						tib.InnerHtml = string.Format("[{0}]",t.Name);
						u.Add (string.Format (
							"{0} <div class=\"{2}\"><code>{3}</code>{1}</div>", 
							tib.ToString (), 
							Parser.ToHtml (temp),
							BBCodeViewClass,
							temp));


				}
				return u.ToArray ();
			}


		}

		static string tagUsage(BBTag t,string content=null)
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("[{0}", t.Name);
			List<string> done = new List<string> ();
			foreach (var a in t.Attributes) {
				if (string.IsNullOrEmpty (a.Name)) {
					sb.AppendFormat ("=default_attribute_value");
					done.Add (a.ID);
				}
			}

			foreach (var a in t.Attributes) {
				if (!done.Contains(a.ID)) { 
					sb.AppendFormat (" {0}=attribute_value", a.ID);
					done.Add (a.ID);
				}
			}
			if (content==null)
				sb.AppendFormat ("]content[/{0}]", t.Name);
			else 
				sb.AppendFormat ("]{1}[/{0}]", t.Name, content);

			if (!parent.ContainsKey(t.Name)) return sb.ToString ();
			return tagUsage (parent [t.Name],sb.ToString());

		}
		private static BBCodeParser parser = null;
		private static int sect1=0;
		private static int sect2=0;
		private static int sect3=0;
		private static Dictionary<string,string> d = new Dictionary<string,string> ();

		/// <summary>
		/// Inits the parser.
		/// </summary>
		public static void InitParser ()
		{
			// prevents a failure at second call
			parent.Clear ();

			BBTag urlBBTag = new BBTag ("url", "<a href=\"${href}\">", "</a>", true, true, UrlContentTransformer, new BBAttribute ("href", ""), new BBAttribute ("href", "href"));

			BBTag bblist =new BBTag ("list", "<ul>", "</ul>");
			BBTag bbs2=new BBTag ("sect2",
				"<div class=\"section\">" +
				"<h2><a name=\"s${para}\" href=\"#${para}\">${para} - ${title}</a></h2>" +
				"<div>${content}",
				"</div></div>", 
				false, true,
				Section2Transformer,
				new BBAttribute ("title", "",TitleContentTransformer),
				new BBAttribute ("title", "title", TitleContentTransformer),
				new BBAttribute ("para", "para", L2ContentTransformer));
			BBTag bbs1=new BBTag ("sect1", 
				"<div class=\"section\">" +
				"<h1><a name=\"s${para}\" href=\"#s${para}\">${para} - ${title}</a></h1>" +
				"<div>${content}",
				"</div></div>", 
				false, true,
				Section1Transformer,
				new BBAttribute ("title", "",TitleContentTransformer),
				new BBAttribute ("title", "title", TitleContentTransformer),
				new BBAttribute ("para", "para", L1ContentTransformer));
			BBTag bbdp=new BBTag ("docpage",
				"<div class=docpage>${content}", "</div>",
				false,
				false,
				DocPageContentTransformer);

			parser = new BBCodeParser (new[] {
				new BBTag ("b", "<b>", "</b>"), 
				new BBTag ("i", "<span style=\"font-style:italic;\">", "</span>"), 
				new BBTag ("em", "<em>", "</em>"), 
				new BBTag ("u", "<span style=\"text-decoration:underline;\">", "</span>"), 
				new BBTag ("code", "<span class=\"code\">", "</span>"), 
				new BBTag ("img", "<img src=\"${content}\" alt=\"${alt}\" style=\"${style}\" />", "", false, true, new BBAttribute ("alt", ""), new BBAttribute("alt","alt"), new BBAttribute ("style", "style")), 
				new BBTag ("quote", "<blockquote>", "</blockquote>"), 
				new BBTag ("div", "<div style=\"${style}\">", "</div>", new BBAttribute("style","style")), 
				new BBTag ("p", "<p>", "</p>"), 
				new BBTag ("h", "<h2>", "</h2>"), 
				bblist, 
				new BBTag ("*", "<li>", "</li>", true, false), 
				urlBBTag,
				new BBTag ("br", "<br/>", "", true, false),
				new BBTag ("video", "<video style=\"${style}\" controls>" +
					"<source src=\"${mp4}\" type=\"video/mp4\"/>" +
					"<source src=\"${ogg}\" type=\"video/ogg\"/>" +
					"<source src=\"${webm}\" type=\"video/webm\"/>","</video>", 
					new BBAttribute("mp4","mp4"),
					new BBAttribute("ogg","ogg"),
					new BBAttribute("webm","webm"),
					new BBAttribute("style","style")),
				new BBTag ("tag", "<span class=\"tag\">", "</span>", true, true,
					TagContentTransformer),
				bbs1,
				bbs2,
				new BBTag ("sect3", 
					"<div class=\"section\">" +
					"<h3><a name=\"s${para}\" href=\"#${para}\">${para} - ${title}</a></h3>" +
					"<div>${content}", 
					"</div></div>", 
					false, true,
					Section3Transformer,
					new BBAttribute ("title", "",TitleContentTransformer),
					new BBAttribute ("title", "title", TitleContentTransformer),
					new BBAttribute ("para", "para", L3ContentTransformer)
				),
				bbdp,
				new BBTag ("f", "<iframe src=\"${src}\">", "</iframe>", 
					new BBAttribute ("src", ""), new BBAttribute ("src", "src")), 

			}
			);
			// used to build the doc toc
			parent.Add ("*", bblist);
			parent.Add ("sect3", bbs2);
			parent.Add ("sect2", bbs1);
			parent.Add ("sect1", bbdp);
			// 

		}
		/// <summary>
		/// Inits the document page.
		/// </summary>
		public static void InitDocPage ()
			{
			sect1 = 0;
			sect2 = 0;
			sect3 = 0;
			d.Clear ();
		}

		static string TocContentTransformer (string instr)
		{
		
			StringBuilder header = new StringBuilder ();
			TagBuilder bshd = new TagBuilder("div");
			bshd.AddCssClass ("bshd");

			header.AppendFormat ("<img src=\"{0}\" alt=\"[Show/Hide TOC]\" class=\"bsh\">\n",
			"/Theme/dark/rect.png");
			StringBuilder ttb = new StringBuilder ();
			int m1=0, m2=0, m3=0;
			foreach (string key in d.Keys) {
				int s1 = 0, s2 = 0, s3 = 0;
				string[] snums = key.Split ('.');
				s1 = int.Parse (snums [0]);
				if (snums.Length>1)
					s2 = int.Parse (snums [1]);
				if (snums.Length>2) {
					s3 = int.Parse (snums [2]);
				}
				if (m1 < s1)
					m1 = s1;
				if (m2 < s2)
					m2 = s2;
				if (m3 < s3)
					m3 = s3;

			}
			string[,,] toc = new string[m1+1, m2+1, m3+1];
			foreach (string key in d.Keys) {
				int s1 = 0, s2 = 0, s3 = 0;
				string[] snums = key.Split ('.');
				s1 = int.Parse (snums [0]);
				if (snums.Length>1)
					s2 = int.Parse (snums [1]);
				if (snums.Length>2) {
					s3 = int.Parse (snums [2]);
				}
				toc [s1, s2, s3] = d [key];
			}

			for (int i = 1; i<=m1; i++) {
				string t1 = toc [i, 0, 0];
				// ASSERT t1 != null
				ttb.AppendFormat ("<a href=\"#s{0}\">{0}) {1}</a><br/>\n", i, t1);

				for (int j = 1; j <= m2; j++) {
					string t2 = toc[i,j,0];
					if (t2 == null)
						break;
					ttb.AppendFormat ("<a href=\"#s{0}.{1}\">{0}.{1}) {2}</a><br/>\n", i,j, t2);

					for (int k = 1; k <= m3; k++) {
						string t3 = toc[i,j,k];
						if (t3 == null)
							break;
						ttb.AppendFormat ("<a href=\"#s{0}.{1}.{2}\">{0}.{1}.{2}) {3}</a><br/>\n", i,j,k,t3);
							
					}
				}
			}
			TagBuilder aside = new TagBuilder ("aside");
			aside.InnerHtml = ttb.ToString ();
			aside.AddCssClass ("bshpanel");
			aside.AddCssClass ("hidden");
			bshd.InnerHtml = header.ToString()+aside.ToString();

			return bshd.ToString ();
		}

		static string DocPageContentTransformer (string instr)
		{
			string toc = TocContentTransformer(instr);
			InitDocPage ();
			return toc+instr;
		}

		static string UrlContentTransformer (string instr)
		{
			if (string.IsNullOrWhiteSpace (instr)) {
				return "-&gt;";
			} else
				return instr;
		}
		static string TagContentTransformer (string instr)
		{
			return instr;
		}

		static string TitleContentTransformer (IAttributeRenderingContext arg)
		{
			string tk;
			if (arg.AttributeValue==null)
				return null;
			string t = arg.AttributeValue;
			t=t.Replace ('_', ' ');
			if (sect3 == 0)
			if (sect2 == 0) {
				tk = string.Format ("{0}", sect1);
			}
			else
				tk = string.Format ("{0}.{1}", sect1 + 1, sect2);
			else
				tk = string.Format ("{0}.{1}.{2}", sect1 + 1, sect2 + 1, sect3);
			if (!d.ContainsKey (tk))
				d.Add (tk, t);
			return t;
		}

		static string Section1Transformer (string instr)
		{
			sect1++;
			sect2 = 0;
			sect3 = 0;
			return instr;
		}

		static string Section2Transformer (string instr)
		{
			sect2++;
			sect3 = 0;
			return instr;
		}

		static string Section3Transformer (string instr)
		{
			sect3++;
			return instr;
		}

		static string L3ContentTransformer (IAttributeRenderingContext arg)
		{
			return (sect1 + 1).ToString () + "." + (sect2 + 1) + "." + sect3;
		}

		static string L2ContentTransformer (IAttributeRenderingContext arg)
		{
			return (sect1 + 1).ToString () + "." + sect2;
		}

		static string L1ContentTransformer (IAttributeRenderingContext arg)
		{
			return sect1.ToString ();
		}
		static string OggSrcTr (IAttributeRenderingContext arg)
		{
			ogg = arg.AttributeValue;
			return ogg;
		}
		static string Mp4SrcTr (IAttributeRenderingContext arg)
		{
			mp4=arg.AttributeValue;
			return mp4;
		}
		static string WebmSrcTr (IAttributeRenderingContext arg)
		{
			webm = arg.AttributeValue;
			return webm;
		}
		static string VideoContentTransformer (string cnt)
		{
			StringBuilder sb = new StringBuilder();
			if (mp4 != null) {
				TagBuilder tb = new TagBuilder ("source");
				tb.Attributes.Add ("src", mp4);
				tb.Attributes.Add ("type", "video/mp4");
				sb.Append (tb.ToString(TagRenderMode.StartTag));
			}
			if (ogg != null) {
				TagBuilder tb = new TagBuilder ("source");
				tb.Attributes.Add ("src", ogg);
				tb.Attributes.Add ("type", "video/ogg");
				sb.Append (tb.ToString(TagRenderMode.StartTag));
			}
			if (webm != null) {
				TagBuilder tb = new TagBuilder ("source");
				tb.Attributes.Add ("src", webm);
				tb.Attributes.Add ("type", "video/webm");
				sb.Append (tb.ToString(TagRenderMode.StartTag));

			}
			mp4 = null;
			 ogg=null;
			webm=null;
			sb.Append (cnt);
			return sb.ToString();
		}
		/// <summary>
		/// Gets the parser.
		/// </summary>
		/// <value>The parser.</value>
		public static BBCodeParser Parser {
			get {
				if (parser == null) {
					InitParser ();
				}
				return parser;
			}

		}
	}
}
