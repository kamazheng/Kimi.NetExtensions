using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace Kimi.NetExtensions.Services;

public static class ApiGenericTable
{
    public static IHttpService Http { get; set; } = AppServicesHelper.Services.GetService(typeof(IHttpService)) as IHttpService ?? throw new Exception("No HttpService Found!");
    public static IJSRuntime jsRuntime { get; set; } = AppServicesHelper.Services.GetService(typeof(IJSRuntime)) as IJSRuntime ?? throw new Exception("No IJSRuntime Found!");

    public static async Task<List<object>> GetItemsAsync(TableQuery postBody) =>
        (List<object>)await Http.Post("GenericTable\\GetItems", postBody, typeof(List<object>));

    public static async Task<object> GetItemAsync(string tableClassFullName, object pk, bool ignoreAutoInclude = false, params string[] includes) =>
            await Http.Post("GenericTable\\GetItem", new RecordQuery
            {
                TableClassFullName = tableClassFullName,
                Id = pk,
                IgnoreAutoInclude = ignoreAutoInclude,
                Includes = includes
            }, typeof(object));

    public static async Task<object> GetItemAsync(RecordQuery query) => await Http.Post("GenericTable\\GetItem", query, typeof(object));

    public static async Task<T> GetItemAsync<T>(object pk, bool ignoreAutoInclude = false, params string[] includes) =>
        await Http.Post<T>("GenericTable\\GetItem", new RecordQuery { TableClassFullName = typeof(T).FullName!, Id = pk, IgnoreAutoInclude = ignoreAutoInclude, Includes = includes });

    public static async Task<List<Dictionary<string, object>>> GetKeyValueItemsAsync(TableQuery postBody) =>
        (List<Dictionary<string, object>>)await Http.Post("GenericTable\\GetItems", postBody, typeof(List<Dictionary<string, object>>));

    public static async Task<List<T>> GetItemsAsync<T>(TableQuery postBody) => await Http.Post<List<T>>("GenericTable\\GetItems", postBody);

    public static async Task<List<T>> GetAllItemsAsync<T>() => await Http.Post<List<T>>("GenericTable\\GetItems",
        new TableQuery { TableClassFullName = typeof(T).FullName });

    public static async Task<object> Upsert(UpsertBody postBody, Type type) => await Http.Put("GenericTable\\Upsert", postBody, type);

    public static async Task<T> Upsert<T>(UpsertBody postBody) => (T)await Upsert(postBody, typeof(T));

    public static async Task<object> Delete(RecordQuery postBody, Type type) => await Http.Delete("GenericTable\\Delete", postBody, type);

    public static async Task<object> Delete(UpsertBody postBody, Type type) => await Http.Delete("GenericTable\\DeleteModel", postBody, type);

    public static async Task<object> DeleteWhere(TableQuery postBody) => await Http.Delete("GenericTable\\DeleteWhere", postBody, typeof(object));

    public static async Task<T> Delete<T>(RecordQuery postBody) => (T)await Delete(postBody, typeof(T));

    public static async Task ExportToExcel(TableQuery postBody)
    {
        var result = await Http.Post("GenericTable\\ExportExcel", postBody, typeof(File));
        var basw64str = Convert.ToBase64String((byte[])result);
        await jsRuntime.InvokeAsync<string>("saveAsFile",
            new object[] { $"{postBody.TableClassFullName}_{DateTime.UtcNow.ToChinaDateTime().ToString("yyyyMMdd_HHmmss")}.xlsx", basw64str });
    }

    public static async Task<string?> ImportExcel(long maxFileSize, IBrowserFile file, string tableFullname)
    {
        var uploadContent = new MultipartFormDataContent();
        var fileContent =
            new StreamContent(file.OpenReadStream(maxFileSize));
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);
        uploadContent.Add(content: fileContent, name: "\"file\"", fileName: tableFullname);
        var response = await Http.Post("GenericTable\\ImportExcel", uploadContent);
        return response.ToString();
    }
}