public static class StreamExtensions
{
    static StreamExtensions() { LicenceHelper.CheckLicense(); }

    public static async Task<string> ConvertToBase64Async(this Stream stream)
    {
        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            bytes = memoryStream.ToArray();
        }

        string base64 = Convert.ToBase64String(bytes);
        return base64;
        //return new MemoryStream(Encoding.UTF8.GetBytes(base64));
    }
}
