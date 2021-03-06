
namespace Yavsc.Interfaces.Workflow {
    public interface IEvent {
        /// <summary>
        /// <c>/topic/(bookquery|estimate)</c>
        /// </summary>
        /// <returns></returns>
        string Topic { get; }
        /// <summary>
        /// Should be the user's name
        /// </summary>
        /// <returns></returns>
        string Sender { get; set ; }

        string CreateBody();
    }


}
