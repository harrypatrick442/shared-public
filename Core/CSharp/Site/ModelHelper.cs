using Core.Exceptions;
namespace Snippets.UnityCore.Site
{
    public static class ModelHelper
    {
        public static string NormalizeMeshName(string meshName) { 
            return meshName.ToLower().Replace("-", "").Replace("_", "").Replace(" ", "");
        }
        public static string NormalizeMasterChoiceType(string masterChoiceType)
        {
            return masterChoiceType.ToLower().Replace("-", "").Replace("_", "").Replace(" ", "");
        }
        public static string RemoveRoomNameFromStartOfMeshName (string nameIncludingRoom, string roomName)
        {
            string nameIncludingRoomNormalized = nameIncludingRoom.ToLower();
            string roomNameNormalized = roomName.ToLower();
            if (nameIncludingRoomNormalized.IndexOf(roomNameNormalized) != 0)
                throw new ParseException($"Could not get the layer from deducting the room name from the layer name provided (expected with room name) \"{nameIncludingRoom}\" for room name \"{roomName}\"");
            string nameWithRoomNameDeducted = nameIncludingRoom.Substring(roomName.Length);
            for (int charIndex = 0; charIndex < nameWithRoomNameDeducted.Length; charIndex++)
            {
                char c = nameWithRoomNameDeducted[charIndex];
                if (c != ' ' && c != '-' && c != '_')
                {
                    return nameWithRoomNameDeducted.Substring(charIndex);
                }
            }
            return nameWithRoomNameDeducted;
        }
    }
}