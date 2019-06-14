namespace Yavsc.Abstract.Chat
{
    public static class ChatHubConstants
    {
        public const string HubGroupAuthenticated = "authenticated";
        public const string HubGroupAnonymous = "anonymous";
        public const string HubGroupCops= "cops";
        public const string HubGroupRomsPrefix = "room_";
        public const int MaxChanelName = 255;

        public const string HubGroupFollowingPrefix = "fol ";
        public const string AnonymousUserNamePrefix = "?";
        public const string KeyParamChatUserName = "username";

        public const string JustCreatedBy = "just created by ";
        public const string LabYouNotOp  = "you're no op.";
        public const string LabNoSuchUser = "No such user";  
        public const string LabNoSuchChan = "No such chan";  
        public const string HopWontKickOp  = "Half operator cannot kick any operator";
        public const string LabAuthChatUser  = "Authenticated chat user";
        public const string NoKickOnCop  = "No, you won´t, you´ĺl never do kick a cop, it is the bad.";
        public const string LabnoJoinNoSend = "LabnoJoinNoSend";
    }
}