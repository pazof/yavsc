


using System;

public static class Html
{
    public static Action<System.IO.TextWriter> Write(object o)
    {
         return new Action<System.IO.TextWriter>(w => w.Write(o));
    }
}