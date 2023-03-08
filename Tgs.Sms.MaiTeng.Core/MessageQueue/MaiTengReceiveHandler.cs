using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Sms.Protos;

namespace Tgs.Sms.MaiTeng.MessageQueue;

public class MaiTengReceiveHandler : IMessageHandler
{
    private readonly ITgsRepository _tgsRepository;
    private readonly ILogger<MaiTengReceiveHandler> _logger;
    private readonly MaiTengClient _maiTengClient;

    public MaiTengReceiveHandler(
        ITgsRepository tgsRepository,
        ILogger<MaiTengReceiveHandler> logger,
        MaiTengClient maiTengClient)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maiTengClient = maiTengClient ?? throw new ArgumentNullException(nameof(maiTengClient));
    }

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var shortMsg = ShortMessage.Parser.ParseFrom(data.Span);
        var brandId = shortMsg.BrandId.ToGuid();

        _logger.LogInformation(
            "Receive sms: [{nameof(shortMsg.BrandId)}] {brandId} " +
            "[{nameof(shortMsg.Destination)}] {shortMsg.Destination} " +
            "[{nameof(shortMsg.Content)}] {shortMsg.Content}",
            nameof(shortMsg.BrandId),
            brandId,
            nameof(shortMsg.Destination),
            shortMsg.Destination,
            nameof(shortMsg.Content),
            shortMsg.Content);

        var brandInfo = await _tgsRepository
            .GetBrandInfoByIdAsync(brandId)
            .ConfigureAwait(false);

        if (brandInfo == null)
        {
            _logger.LogInformation("Can't find brand info ({brandId})", brandId);

            return;
        }

        if (!brandInfo.SmsEnabled || brandInfo.SmsVendorSettings == null)
        {
            _logger.LogWarning("Brand setting warn");

            return;
        }

        var request = new SendSMSRequest
        {
            AppKey = brandInfo.SmsVendorSettings.AppKey,
            AppSecret = brandInfo.SmsVendorSettings.AppSecret,
            AppCode = brandInfo.SmsVendorSettings.AppCode,
            Phone = shortMsg.Destination,
            Msg = shortMsg.Content
        };

        var response = await _maiTengClient
            .SendSMSAsync(request)
            .ConfigureAwait(false);

        if (response is null)
            throw new Exception("Sms send fail");

        _logger.LogInformation("Sms sent.");
    }
}