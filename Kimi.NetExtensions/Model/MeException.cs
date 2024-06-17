namespace Kimi.NetExtensions.Model;

/// <summary>
/// throw this exception to show warning dialog
/// </summary>
public class WarningDialogException : Exception
{
    public WarningDialogException(string? message) : base(message)
    {
    }
}

/// <summary>
/// throw this exception to show error dialog
/// </summary>
public class ErrorDialogException : Exception
{
    public ErrorDialogException(string? message) : base(message)
    {
    }
}

/// <summary>
/// throw this exception to show toast message
/// </summary>
public class ToastException : Exception
{
    public ToastException(string? message) : base(message)
    {
    }

    public ToastException() : base()
    {
    }
}

public class ErrorMessageException : Exception
{
    public ErrorMessageException(string? message) : base(message)
    {
    }
}