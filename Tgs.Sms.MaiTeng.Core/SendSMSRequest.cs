namespace Tgs.Sms.MaiTeng;
public record SendSMSRequest
{
    public string AppKey { get; init; } = default!;
    public string AppSecret { get; init; } = default!;
    public string AppCode { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string Msg { get; init; } = default!;
}