// Define an interface for HttpLog
public interface IHttpLog
{
    // Define the properties and methods for HttpLog
    int Id { get; set; }

    string ExceptionType { get; set; }
    string RequestPath { get; set; }
    string? RequestBody { get; set; }
    string? ResponseBody { get; set; }
    string ErrorMessage { get; set; }
    string? User { get; set; }
    string Updatedby { get; set; }
    DateTime Updated { get; set; }
    bool Active { get; set; }

    Task SaveAsync();

    Task<bool> IsLogOnAsync();
}