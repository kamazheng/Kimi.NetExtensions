using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Reflection;

public static class ExcelService
{
    static ExcelService()
    {
        LicenceHelper.CheckLicense();
    }

    /// <summary>
    /// Get excel file for controller downloading
    /// </summary>
    /// <param name="list"></param>
    /// <param name="fileName"></param>
    /// <param name="isReadableFieldsOnly"></param>
    /// <returns></returns>
    public static async Task<FileContentResult> GetExcelFile(List<object> list, string fileName, bool isReadableFieldsOnly = true)
    {
        var file = await Task.Run(() => ExcelService.GenerateExcelWorkbook(list, isReadableFieldsOnly));
        var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return new FileContentResult(file, mimeType) { FileDownloadName = fileName };
    }

    /// <summary>
    /// Get raw excel file from list of object
    /// </summary>
    /// <param name="list"></param>
    /// <param name="isReadableFieldsOnly"></param>
    /// <returns></returns>
    public static byte[] GenerateExcelWorkbook(List<object> list, bool isReadableFieldsOnly = true)
    {
        var _workbook = new XSSFWorkbook(); //Creating New Excel object
        var _sheet = _workbook.CreateSheet("Sheet1"); //Creating New Excel Sheet object

        var headerStyle = _workbook.CreateCellStyle(); //Formatting
        var headerFont = _workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        var classType = list[0].GetType();

        PropertyInfo[] properties = classType.GetProperties();
        if (isReadableFieldsOnly)
        {
            properties = properties.Where(p => p.IsCollectible)
                .Where(p => !p.PropertyType.IsClass || p.PropertyType == typeof(string) || p.PropertyType == typeof(object))
                .Where(p => !p.PropertyType.IsGenericType || !typeof(IEnumerable<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
                .Where(p => !p.PropertyType.IsGenericType || !typeof(ICollection<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
                .ToArray();
        }

        // create the header row
        IRow headerRow = _sheet.CreateRow(0);

        // populate the header row with property names
        for (int i = 0; i < properties.Length; i++)
        {
            ICell cell = headerRow.CreateCell(i);
            cell.SetCellValue(properties[i].Name);
        }

        // populate the worksheet with data from the list
        for (int i = 0; i < list.Count; i++)
        {
            // create a new row
            IRow row = _sheet.CreateRow(i + 1);
            // populate the row with data from the object
            for (int j = 0; j < properties.Length; j++)
            {
                ICell cell = row.CreateCell(j);
                cell.SetCellValue(properties[j]?.GetValue(list[i])?.ToString());
            }
        }
        using (var memoryStream = new MemoryStream()) //creating memoryStream
        {
            _workbook.Write(memoryStream);
            return memoryStream.ToArray();
        };
    }

    /// <summary>
    /// Get object from excel sheet, for example:
    /// IWorkbook workbook;
    /// using (var stream = file.OpenReadStream())
    /// workbook = WorkbookFactory.Create(stream);
    /// ISheet sheet = workbook.GetSheetAt(0);
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="objecType"></param>
    /// <returns></returns>
    public static List<object> GetList(this ISheet sheet, Type objecType)
    {
        List<object> list = new List<object>();
        //first row is for knowing the properties of object
        var columnInfo = Enumerable.Range(0, sheet.GetRow(0).LastCellNum).ToList().Select(n =>
            new { Index = n, ColumnName = sheet.GetRow(0).GetCell(n).StringCellValue }
        );

        for (int row = 1; row <= sheet.LastRowNum; row++)
        {
            var obj = Activator.CreateInstance(objecType)!;//generic object
            foreach (var prop in objecType.GetProperties())
            {
                var col = columnInfo.SingleOrDefault(c => c.ColumnName.ToLower() == prop.Name.ToLower())?.Index;
                if (col != null)
                {
                    var val = sheet.GetRow(row)?.GetCell(col.Value)?.ToString();
                    obj.SetPropertyValue(prop.Name, val);
                }
            }
            list.Add(obj);
        }
        return list;
    }
}