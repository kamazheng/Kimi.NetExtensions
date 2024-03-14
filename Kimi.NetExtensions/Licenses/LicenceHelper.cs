using Newtonsoft.Json;
using System.Reflection;
using System.Text;

public static class LicenceHelper
{
    private static string? encryptLicenseCode;
    private static string? hostMachine;
    private static string? appName;
    private static string? publicKey;
    private static string? licenseServer;

    /// <summary>
    ///
    /// </summary>
    /// <param name="_licenseServer"></param>
    public static void SetParameter(string? _licenseServer = null)
    {
        hostMachine = Environment.MachineName;
        appName = Assembly.GetEntryAssembly()?.GetName()?.Name;
        licenseServer = _licenseServer ?? "https://molexchengduqa.azurewebsites.net/License/GetLicense";
        GetReleasedLicense();
    }

    private static void GetReleasedLicense()
    {
        var postBody = new RequestBody
        {
            HostMachine = hostMachine!,
            AppName = appName!,
            IsDebug = EnvironmentExtension.IsDebug
        };
        using (var client = new HttpClient())
        {
            var content = new StringContent(JsonConvert.SerializeObject(postBody), Encoding.UTF8, "application/json");
            var result = AsyncUtil.RunSync(() => client.PostAsync(licenseServer, content));
            var response = AsyncUtil.RunSync(() => result.Content.ReadAsStringAsync());
            if (result.IsSuccessStatusCode)
            {
                var releaseLicense = JsonConvert.DeserializeObject<ReleaseLicense>(response);
                if (releaseLicense != null)
                {
                    encryptLicenseCode = releaseLicense.EncriptLicense;
                    publicKey = releaseLicense.PublicKey;
                }
            }
        }
    }

    public static void SetParameter(string licenseCode, string publickey)
    {
        hostMachine = Environment.MachineName;
        appName = Assembly.GetEntryAssembly()?.GetName()?.Name;
        encryptLicenseCode = licenseCode;
        publicKey = publickey;
    }

    public static void CheckLicense(string? encryptLicene)
    {
        if (encryptLicene is null) { throwRandomException(); }
        var licenseStr = RSAHelper.PublicKeyDecrypt(publicKey, encryptLicene!);
        var license = JsonConvert.DeserializeObject<License>(licenseStr);
        if (license == null || license.HostMachine != hostMachine || license.AppName != appName || license.ExpiredUtcTime == default)
        { throwRandomException(); }
        var balanceDays = (license!.ExpiredUtcTime.Date - DateTime.Now.Date).TotalDays;
        if (NeedToBreakApp((int)balanceDays, 10))
        {
            throwRandomException();
            return;
        }
    }

    private static void throwRandomException()
    {
        var exceptions = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(Exception)) && t.GetConstructor(Type.EmptyTypes) != null)
            .ToList();
        Random rnd = new Random();
        int index = rnd.Next(exceptions.Count);
        var exception = (Exception)Activator.CreateInstance(exceptions[index])!;
        throw new Exception(exception.Message + "!");
    }

    public static void CheckLicense()
    {
        CheckLicense(encryptLicenseCode);
    }

    static bool NeedToBreakApp(int balanceDays, int expandDays)
    {
        if (balanceDays > 0) { return false; }
        var randomDays = new Random().Next(0, expandDays);
        if (Math.Abs(balanceDays) > randomDays)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class License
{
    public int Id { get; set; }

    public string HostMachine { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public bool IsDebug { get; set; }
    public DateTime ExpiredUtcTime { get; set; }
    public DateTime CreatedUtcTime { get; set; }
}

public class ReleaseLicense
{
    public string PublicKey { get; set; } = default!;
    public string EncriptLicense { get; set; } = default!;
}

public class RequestBody
{
    public string HostMachine { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public bool IsDebug { get; set; } = EnvironmentExtension.IsDebug;
}