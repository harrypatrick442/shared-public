using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using Core.Enums;
using System.Collections.Generic;

namespace Core.SnippetsOpenRouting
{
    [DataContract]
    public class UsersWithSnippetOpen
    {
        private Dictionary<long, UserIdPermissionsAndSessionId> _MapUserIdToPermissionsAndSessionIds;
        [JsonPropertyName(UsersWithSnippetOpenDataMemberNames.Entries)]
        [JsonInclude]
        [DataMember(Name = UsersWithSnippetOpenDataMemberNames.Entries)]
        protected UserIdPermissionsAndSessionId[] Entries
        {
            get
            {
                return _MapUserIdToPermissionsAndSessionIds.Values?.ToArray();
            }
            set
            {
                _MapUserIdToPermissionsAndSessionIds = value?.Where(u=>u!=null).ToDictionary(u=>u.UserId, u=>u);
            }
        }
        public void Add(long userId, long sessionId, SnippetConnectionFlag permissions) {
                if (_MapUserIdToPermissionsAndSessionIds == null)
                {
                    _MapUserIdToPermissionsAndSessionIds = new Dictionary<long, UserIdPermissionsAndSessionId> { { userId, new UserIdPermissionsAndSessionId(userId, sessionId, permissions) } };
                    return;
                }
                if (_MapUserIdToPermissionsAndSessionIds.TryGetValue(userId,
                    out UserIdPermissionsAndSessionId userIdPermissionsAndSessionId))
                {
                    userIdPermissionsAndSessionId.Add(sessionId, permissions);
                    return;
                }
                _MapUserIdToPermissionsAndSessionIds[userId] = new UserIdPermissionsAndSessionId(userId, sessionId, permissions); ;
        }
        public bool Remove(long userId, long sessionId) {
                if (_MapUserIdToPermissionsAndSessionIds == null) return true;
                if(_MapUserIdToPermissionsAndSessionIds.TryGetValue(userId,
                    out UserIdPermissionsAndSessionId userIdPermissionsAndSessionId))
                {
                    if (!userIdPermissionsAndSessionId.Remove(sessionId))
                        return false;
                    _MapUserIdToPermissionsAndSessionIds.Remove(userId);
                    if (!_MapUserIdToPermissionsAndSessionIds.Any())
                        return true;
                }
                return !_MapUserIdToPermissionsAndSessionIds.Any();
        }
        public long[] GetUserIds()
        {
            return _MapUserIdToPermissionsAndSessionIds.Keys.ToArray();
        }
        public SnippetConnectionFlag? GetPermissions(long userId)
        {

            if (_MapUserIdToPermissionsAndSessionIds.TryGetValue(userId,
                out UserIdPermissionsAndSessionId userIdPermissionsAndSessionId))
            {
                return userIdPermissionsAndSessionId.Permissions;
            }
            return null;
        }
        public UsersWithSnippetOpen() { }
    }
}
