namespace Yavsc.Server
{
    public interface ITranslator
    {
        string[] Translate (string slang, string dlang, string[] text);
    }
}
