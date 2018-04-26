using System;
using System.Text;
using System.Threading.Tasks;

namespace Yavsc.Abstract.Templates
{
  /// <summary>
  /// A CSharp Razor template.
  /// </summary>
  public abstract class Template
  {

    StringBuilder _buffer = new StringBuilder();


    public virtual void Write(object value)
    { WriteLiteral(value); }

    public virtual void WriteLiteral(object value)
    { _buffer.Append(value); }

    public string GeneratedText {
      get {
        return _buffer.ToString();
      }
    }

    public abstract Task ExecuteAsync();

  }
}
