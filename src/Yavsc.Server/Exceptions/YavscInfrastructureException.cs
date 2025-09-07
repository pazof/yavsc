namespace Yavsc.Services
{
    [Serializable]
    internal class YavscInfrastructureException : Exception
    {
        public YavscInfrastructureException()
        {
        }

        public YavscInfrastructureException(string? message) : base(message)
        {
        }

        public YavscInfrastructureException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
