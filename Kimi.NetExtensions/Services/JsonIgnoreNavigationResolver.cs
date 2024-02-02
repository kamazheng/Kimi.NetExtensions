using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

internal class JsonIgnoreNavigationResolver : DefaultContractResolver
{
    /// <summary>
    /// Ignore the virtual property of object
    /// </summary>
    /// <param name="member"></param>
    /// <param name="memberSerialization"></param>
    /// <returns></returns>
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty prop = base.CreateProperty(member, memberSerialization);

        var propInfo = member as PropertyInfo;

        if (propInfo != null)
        {
            // Add a null check for the GetMethod property
            if (propInfo.GetMethod != null && propInfo.GetMethod.IsVirtual && !propInfo.GetMethod.IsFinal)
            {
                prop.ShouldSerialize = obj => false;
            }
        }

        return prop;
    }

}