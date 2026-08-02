namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for ObjectExtensions.
/// </summary>
public static class ObjectExtensions
{
        /// <summary>
    /// ToJson method.
    /// </summary>
public static string ToJson(this object obj) 
        => JsonSerializer.Serialize(obj);
    
        /// <summary>
    /// FromJson method.
    /// </summary>
public static T? FromJson<T>(string json) where T : class
    {
        return JsonSerializer.Deserialize<T>(json);
    }
    
    /// <summary>
    /// Converts an object to a json string for debugging / logging purposes
    /// </summary>
    /// <param name="obj">the object to serialize</param>
    /// <returns>a json string</returns>
    public static string Dump(this object obj) => obj.ToJson();
    
        /// <summary>
    /// ToQueryString method.
    /// </summary>
public static string ToQueryString(this object obj)
    {
        if (obj == null)
            return "";
            
        var list = new List<KeyValuePair<string, string>>();
        var properties = obj.GetType().GetProperties();
            
        foreach (var propertyInfo in properties)
        {
            var value = propertyInfo.GetValue(obj);
            if (value == null)
                continue;

            var collection = value as System.Collections.ICollection;
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    list.Add(new KeyValuePair<string, string>(propertyInfo.Name, item.ToString()));
                }
            }
            else
            {
                list.Add(new KeyValuePair<string, string>(propertyInfo.Name, value.ToString()));
            }
        }

        return string.Join("&", list.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}