public class Email
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public List<string> To { get; set; } = new List<string>();
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public List<Attachment>? Attachments { get; set; }
}

public class Attachment
{
    public string Name { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
}