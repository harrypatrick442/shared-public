using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using Core.Enums;
using System.Collections.Generic;

namespace Core.SnippetsOpenRouting
{
    [DataContract]
    public class UserIdPermissionsAndSessionId
    {
        private long _UserId;
        [JsonPropertyName(UserIdPermissionsPairDataMemberNames.UserId)]
        [JsonInclude]
        [DataMember(Name = UserIdPermissionsPairDataMemberNames.UserId)]
        public long UserId { get { return _UserId; } protected set { _UserId = value; } }
        private HashSet<long> _SessionIds;
        [JsonPropertyName(UserIdPermissionsPairDataMemberNames.SessionIds)]
        [JsonInclude]
        [DataMember(Name = UserIdPermissionsPairDataMemberNames.SessionIds)]
        public long[] SessionIds { 
            get { return _SessionIds?.ToArray(); }
            protected set { _SessionIds = value==null?null:new HashSet<long>(value); }
        }

        private SnippetConnectionFlag _Permissions;
        [JsonPropertyName(UserIdPermissionsPairDataMemberNames.Permissions)]
        [JsonInclude]
        [DataMember(Name = UserIdPermissionsPairDataMemberNames.Permissions)]
        public SnippetConnectionFlag Permissions
        {
            get { return _Permissions; }
            protected set { _Permissions = value; }
        }
        public void Add(long sessionId, SnippetConnectionFlag permissions) {
            _Permissions = _Permissions | permissions;
            if (_SessionIds.Contains(sessionId)) return;
            _SessionIds.Add(sessionId);
        }
        public bool Remove(long sessionId) {
            _SessionIds.Remove(sessionId);
            return !_SessionIds.Any();
        }
        public UserIdPermissionsAndSessionId(long userId, 
            long sessionId, SnippetConnectionFlag permissions)
        {
            _UserId = userId;
            _Permissions = permissions;
            _SessionIds = new HashSet<long> { sessionId };
        }
        protected UserIdPermissionsAndSessionId() { }
    }
}
