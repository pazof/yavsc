using System;
using Xamarin.Forms;

namespace ZicMoove.Interfaces
{
    public interface IMDEditor
    {
        View EditorView { get; }
        string Text { get; set; }
        bool Editable { get; set; }
        Action<string> Edited { get; set; }
    }
}
