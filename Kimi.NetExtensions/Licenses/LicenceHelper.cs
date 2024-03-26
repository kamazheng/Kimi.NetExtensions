using Newtonsoft.Json;
using System.Reflection;
using System.Text;

public static class LicenceHelper
{
    private static string? encryptLicenseCode;
    private static string? hostMachine;
    private static string? appName;

    private static DateTime lstCheckTime = DateTime.MinValue;

    private static string publicKey { get; set; } = """
        <RSAKeyValue><Modulus>mlFF55YRO8o/IYyfU8t9m53JkFR5UKgek5CuL5WZ9tcup2A4m+VokFiWmoiBrt9u/o/FIcmyVstcWB0T+TMX8zVIijVKzf4M9PlOOKe7dXdqOGujhufzu34Mj5MC1B2OYcygHuIrD7fyAw2B/H0hPEi1cJ91RP8akQ2bV7i95m0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>
        """;

    private static string? licenseServer;

    /// <summary>
    ///
    /// </summary>
    /// <param name="_licenseServer"></param>
    public static void SetParameterByServer(string? _licenseServer = null)
    {
        hostMachine = Environment.MachineName;
        appName = Assembly.GetEntryAssembly()?.GetName()?.Name;
        licenseServer = _licenseServer ?? "https://molexchengduqa.azurewebsites.net/License/GetLicense";
        GetReleasedLicense();
    }

    private static void GetReleasedLicense()
    {
        try
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
                        lstCheckTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    getReleasedLicenseFromLocal();
                    lstCheckTime = DateTime.UtcNow;
                }
            }
        }
        catch (Exception ex)
        {
            getReleasedLicenseFromLocal();
            lstCheckTime = DateTime.UtcNow;
        }
    }

    private static void getReleasedLicenseFromLocal()
    {
        var releaseLicense = ConfigReader.GetSettings<ReleaseLicense>();
        publicKey = releaseLicense?.PublicKey ?? publicKey;
        encryptLicenseCode = releaseLicense?.EncriptLicense;
    }

    public static void SetParameterByServerByLicenseCode(string licenseCode)
    {
        hostMachine = Environment.MachineName;
        appName = Assembly.GetEntryAssembly()?.GetName()?.Name;
        encryptLicenseCode = licenseCode;
    }

    private static void CheckLicense(string? encryptLicene)
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
        if (DateTime.UtcNow.Subtract(lstCheckTime).TotalHours > 6)
        {
            if (!string.IsNullOrEmpty(licenseServer))
            {
                GetReleasedLicense();
            }
        }
        CheckLicense(encryptLicenseCode);
        lstCheckTime = DateTime.UtcNow;
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