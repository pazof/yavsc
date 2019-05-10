namespace Yavsc
{
    public static class ServerCommands {

        public const string whois = nameof(whois);
        public const string whowas = nameof(whowas);

        /// <summary>
        /// modify a chan or an user
        /// </summary>
        /// <returns></returns>
        public const string mode = nameof(mode);

        /// <summary>
        /// register a channel
        /// </summary>
        /// <returns></returns>
        public const string register = nameof(register);

        /// <summary>
        /// kick some user
        /// </summary>
        /// <returns></returns>
        public const string kick = nameof(kick);
        
        /// <summary>
        /// ban an user
        /// </summary>
        /// <returns></returns>
        public const string ban = nameof(ban);

    }
}
