
public static class DictionaryExtensions
{
    static DictionaryExtensions() { LicenceHelper.CheckLicense(); }

    public static Dictionary<string, T> ToCaseInsensitive<T>(this Dictionary<string, T> dict)
    {
        // Create a new case-insensitive dictionary
        var newDict = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

        // Copy data from old dictionary to new dictionary
        foreach (var kvp in dict)
        {
            newDict.Add(kvp.Key, kvp.Value);
        }

        return newDict;
    }

    public static List<Dictionary<string, T>> ToCaseInsensitive<T>(this List<Dictionary<string, T>> list)
    {
        // Create a new list of case-insensitive dictionaries
        var newList = new List<Dictionary<string, T>>();

        // Copy data from old list to new list
        foreach (var oldDict in list)
        {
            // Create a new case-insensitive dictionary
            var newDict = oldDict.ToCaseInsensitive();

            // Add the new dictionary to the new list
            newList.Add(newDict);
        }

        return newList;
    }
}

