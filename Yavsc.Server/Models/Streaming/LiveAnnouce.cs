
namespace Yavsc.ViewModels.Streaming
{

    public class LiveAnnouce {

      // set by the server, unique
      long Id { get; set; }

      // a title for this flow
      string Title { get; set; }

      // a little description
      string Pitch { get; set; }

      // The stream type
      string MediaType { get; set; }

      // A name where to save this stream, relative to user's files root
      string DifferedFileName { get; set; }

    }
}     
