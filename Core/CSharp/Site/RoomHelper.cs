
namespace Snippets.UnityCore.Site
{
    public static class RoomHelper
    {
        public static string NormalizeRoomName(string roomName) {
            return roomName.ToLower().Replace("-", "").Replace("_", "").Replace(" ", "");
        }
    }
}