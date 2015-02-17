using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Text input.
	/// </summary>
	public class TextInput:FormInput
	{

		#region implemented abstract members of FormInput
		private string tpe = null;
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public override string Type {
			get {
				return tpe;
			}
		}
		#endregion
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.TextInput"/> class.
		/// </summary>
		public TextInput ()
		{
			tpe = "text";
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.TextInput"/> class.
		/// </summary>
		/// <param name="txt">Text.</param>
		public TextInput (string txt)
		{
			tpe = "text";
			text = txt;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.TextInput"/> class.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="txt">Text.</param>
		public TextInput (string type, string txt)
		{
			tpe = type;
			text = txt;
		}

		string text = null;

		/// <param name="t">T.</param>
		public static implicit operator string(TextInput t)
		{
			return t.text;
		}
		/// <param name="t">T.</param>
		public static implicit operator TextInput(string t)
		{
			return new TextInput(t);
		}
		/// <summary>
		/// Gets or sets the default value.
		/// </summary>
		/// <value>The default value.</value>
		public string DefaultValue {
			get {
				return text;
			}
			set {
				text = (string) value;
			}
		}

		private bool multiline = false;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.FrontOffice.TextInput"/> multi line.
		/// </summary>
		/// <value><c>true</c> if multi line; otherwise, <c>false</c>.</value>
		public bool MultiLine { get { return multiline; } set { multiline = value; } }

		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{

			return MultiLine?
				string.Format ("<textarea id=\"{0}\" name=\"{1}\">{2}</textarea>", Id,Name,DefaultValue)
				: string.Format ("<input type=\"text\" id=\"{0}\" name=\"{1}\" value=\"{2}\"/>", Id,Name,DefaultValue);
		}
	}
}

