namespace Tgs.Sms.MaiTeng;
public record BrandInfo(
    Guid Id,
    string Name,
    bool SmsEnabled,
    string? SmsVendorCode,
    MaiTengSettings? SmsVendorSettings);