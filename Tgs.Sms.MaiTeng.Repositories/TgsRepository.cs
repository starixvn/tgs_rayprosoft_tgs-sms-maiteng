using Tgs.Sms.MaiTeng.Repositories.Entities;

namespace Tgs.Sms.MaiTeng.Repositories;

internal class TgsRepository : ITgsRepository
{
    private readonly TgsContext _tgsContext;
    private readonly MaiTengSerializer _maiTengSerializer;

    public TgsRepository(
        TgsContext tgsContext,
        MaiTengSerializer maiTengSerializer)
    {
        _tgsContext = tgsContext ?? throw new ArgumentNullException(nameof(tgsContext));
        _maiTengSerializer = maiTengSerializer ?? throw new ArgumentNullException(nameof(maiTengSerializer));
    }

    public async ValueTask<BrandInfo?> GetBrandInfoByIdAsync(Guid brandId)
    {
        var brand = await _tgsContext.Brands
            .FindAsync(brandId)
            .ConfigureAwait(false);

        if (brand == null)
            return null;

        return new BrandInfo(
            brand.Id,
            brand.Code,
            brand.EnableSms,
            brand.SmsVendorCode,
            string.IsNullOrWhiteSpace(brand.SmsVendorSettings)
                ? null
                : _maiTengSerializer.Deserialize<MaiTengSettings>(brand.SmsVendorSettings));
    }
}