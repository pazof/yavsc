

public interface IEvent {
    /// <summary>
    /// An acceptable topic for this event to be.
    /// Should return something like the class name
    /// of this object
    /// </summary>
    /// <returns></returns>
    string Topic { get; set ; }
    string Sender { get; set ; }

    string Message { get; set; }
}