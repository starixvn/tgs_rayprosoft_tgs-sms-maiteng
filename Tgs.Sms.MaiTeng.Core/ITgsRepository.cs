namespace Tgs.Sms.MaiTeng;

public interface ITgsRepository
{
    public ValueTask<BrandInfo?> GetBrandInfoByIdAsync(Guid brandId);
}