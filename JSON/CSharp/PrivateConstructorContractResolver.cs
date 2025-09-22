using System.Text.Json;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
namespace JSON
{
    public class PrivateConstructorContractResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object && jsonTypeInfo.CreateObject is null)
            {
                bool hasNoParameterlessPublicConstructors = 
                    !jsonTypeInfo.Type
                    .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Where(c => !c.GetParameters().Any())
                    .Any();
                if (hasNoParameterlessPublicConstructors)
                {
                    // The type doesn't have public constructors
                    jsonTypeInfo.CreateObject = () =>
                        Activator.CreateInstance(jsonTypeInfo.Type, true);
                }
            }
            return jsonTypeInfo;
        }
    }
}
