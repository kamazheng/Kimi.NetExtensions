using System.Data;
using System.Dynamic;

public static class DataTableExtentions
{
    static DataTableExtentions()
    {
        LicenceHelper.CheckLicense();
    }

    public static List<T> MapTableToList<T>(this DataTable table) where T : new()
    {
        List<T> result = new List<T>();
        var Type = typeof(T);

        foreach (DataRow row in table.Rows)
        {
            T item = new T();
            foreach (var property in Type.GetProperties())
            {
                if (table.Columns[property.Name] != null)
                {
                    item.SetPropertyValue(property.Name, row[table.Columns[property.Name]!]);
                }
            }
            result.Add(item);
        }
        return result;
    }

    public static List<object> MapTableToList(this DataTable table, Type objectType)
    {
        List<object> result = new List<object>();

        foreach (DataRow row in table.Rows)
        {
            //var item = new ExpandoObject();
            var item = Activator.CreateInstance(objectType)!;
            foreach (var property in objectType.GetProperties())
            {
                if (table.Columns[property.Name] != null)
                {
                    //((IDictionary<string, object?>)item)[property.Name] = row[table.Columns[property.Name]!];
                    item.SetPropertyValue(property.Name, row[table.Columns[property.Name]!]);
                }
            }
            result.Add(item);
        }
        return result;
    }

    public static List<ExpandoObject> ToDynamicList(this DataTable dt)
    {
        var list = new List<ExpandoObject>();

        foreach (DataRow row in dt.Rows)
        {
            dynamic expandoObject = new ExpandoObject();
            var expandoDictionary = expandoObject as IDictionary<string, object>;

            foreach (DataColumn col in dt.Columns)
            {
                expandoDictionary[col.ColumnName] = row[col];
            }

            list.Add(expandoObject);
        }

        return list;
    }
}