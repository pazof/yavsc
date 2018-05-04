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

    StringBuilder _buffer ;

    public virtual void Write(object value)
    { WriteLiteral(value); }

    public virtual void WriteLiteral(object value)
    { _buffer.Append(value); }

    public string GeneratedText {
      get {
        return _buffer.ToString();
      }
    }

    public virtual void Init() {
       _buffer = new StringBuilder();
    }
    
    public abstract Task ExecuteAsync();

  }
}
