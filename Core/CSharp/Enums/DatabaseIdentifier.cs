namespace Core.Enums {
    public enum DatabaseIdentifier
    {
        Snippets = 1,
        SnippetIdToName = 2,
        GuidToAuthenticationToken = 3,
        UserIdToAuthentication = 4,
        PhoneToAuthenticationInfo = 5,
        UserIdToAuthenticationInfo = 6,
        EmailToAuthenticationInfo = 7,
        UserIdToClientState=8,
        UsersWithSnippetOpen = 13,
        UserIdToAssociateRequestsSent=9,
        UserIdToAssociateRequestsReceived=10,
        UserIdToUserProfile=11,
        UserIdToAssociates=12,
        UserIdToConversations=14,
        ConversationIdToConversation=15,
        UserIdHighest_LowestToPm=16,
        Messages=17,
        UserIdToFrequentlyAccessedUserProfile=18,
        StackHashToError=19,
        MessageHashToError=20,
        BrowserToError=21,
        PlatformToError=22,
        BrowserToSession=23,
        PlatformToSession=24,
        SessionToErrors=25,
        SessionToBreadcrumbs=26,
        BreadcrumbTypeIdToBreadcrumbs =27,
        BreadcrumbValueHashToBreadcrumbs =28,
        UsernameToAuthenticationInfo=29,
        UserQuadTree=30,
        UserIdToUserIgnores = 31,
        UserIdToBeingIgnoredBys = 32,
        UserIdToUserNotifications=33
    }
    public static class DatabaseIdentifierExtensions
    {
        public static int Int(this DatabaseIdentifier value){
            return (int)value;
        }
    }
}

