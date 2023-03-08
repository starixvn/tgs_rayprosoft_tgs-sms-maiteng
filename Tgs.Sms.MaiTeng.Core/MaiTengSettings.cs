namespace Tgs.Sms.MaiTeng;
public record MaiTengSettings
{
    public string AppKey { get; init; } = default!;
    public string AppSecret { get; init; } = default!;
    public string AppCode { get; init; } = default!;
}