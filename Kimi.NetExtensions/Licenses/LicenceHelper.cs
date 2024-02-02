using Newtonsoft.Json;

public static class LicenceHelper
{
    private static string? encryptLicenseCode;
    private static string? companyName;
    private static string? appName;

    /// <summary>
    /// Set License paramater, reading from setting files. Json Key - KimiExtension:LicenseCode,CompanyNamem,AppName
    /// </summary>
    public static void SetParameter()
    {
        encryptLicenseCode = ConfigReader.Configuration["KimiExtension:LicenseCode"];
        companyName = ConfigReader.Configuration["KimiExtension:CompanyName"]; ;
        appName = ConfigReader.Configuration["KimiExtension:AppName"]; ;
    }

    public static void SetParameter(string licenseCode, string company, string app)
    {
        encryptLicenseCode = licenseCode;
        companyName = company;
        appName = app;
    }

    public static void CheckLicense(string? encryptLicene)
    {
        if (encryptLicene is null) { throwRandomException(); }
        var licenseStr = RSAHelper.PublicKeyDecrypt(RSAHelper.PublicKey, encryptLicene!);
        var license = JsonConvert.DeserializeObject<License>(licenseStr);
        if (license == null || license.Company != companyName || license.AppName != appName || license.ExpiredDate == default)
        { throwRandomException(); }
        var balanceDays = (license!.ExpiredDate.Date - DateTime.Now.Date).TotalDays;
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
    public string Company { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public DateTime ExpiredDate { get; set; }
}