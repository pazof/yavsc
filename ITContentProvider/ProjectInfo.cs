using System;
using Yavsc;

namespace Yavsc.Model
{
	/// <summary>
	/// Project info.
	/// </summary>
	public class ProjectInfo
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; set; }
		/// <summary>
		/// Gets or sets the licence.
		/// </summary>
		/// <value>The licence.</value>
		string Licence { get; set; }
		/// <summary>
		/// Gets or sets the BB description.
		/// </summary>
		/// <value>The BB description.</value>
		string BBDescription { get; set; }
		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		/// <value>The start date.</value>
		DateTime StartDate { get; set; }
		/// <summary>
		/// Gets or sets the prod version.
		/// </summary>
		/// <value>The prod version.</value>
		string ProdVersion { get; set; }
		/// <summary>
		/// Gets or sets the stable version.
		/// </summary>
		/// <value>The stable version.</value>
		string StableVersion { get; set; }
		/// <summary>
		/// Gets or sets the testing version.
		/// </summary>
		/// <value>The testing version.</value>
		string TestingVersion { get; set; }
		/// <summary>
		/// Gets or sets the web site.
		/// </summary>
		/// <value>The web site.</value>
		string WebSite { get; set; }
	}
}

