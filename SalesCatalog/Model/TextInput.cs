using System;

namespace SalesCatalog.Model
{
	public class TextInput:FormInput
	{

		#region implemented abstract members of FormInput
		private string tpe = null;
		public override string Type {
			get {
				return tpe;
			}
		}
		#endregion

		public TextInput ()
		{
			tpe = "text";
		}

		public TextInput (string txt)
		{
			tpe = "text";
			text = txt;
		}

		public TextInput (string type, string txt)
		{
			tpe = type;
			text = txt;
		}

		string text = null;


		public static implicit operator string(TextInput t)
		{
			return t.text;
		}

		public static implicit operator TextInput(string t)
		{
			return new TextInput(t);
		}

		public string DefaultValue {
			get {
				return text;
			}
			set {
				text = (string) value;
			}
		}

		private bool multiline = false;
		public bool MultiLine { get { return multiline; } set { multiline = value; } }

		public override string ToHtml ()
		{

			return MultiLine?
				string.Format ("<textarea id=\"{0}\" name=\"{1}\">{2}</textarea>", Id,Name,DefaultValue)
				: string.Format ("<input type=\"text\" id=\"{0}\" name=\"{1}\" value=\"{2}\"/>", Id,Name,DefaultValue);
		}
	}
}

