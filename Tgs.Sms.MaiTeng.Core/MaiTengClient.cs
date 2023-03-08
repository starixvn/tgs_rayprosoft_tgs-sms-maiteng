using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Tgs.Sms.MaiTeng.Models;

namespace Tgs.Sms.MaiTeng;

public class MaiTengClient
{
    private static readonly Dictionary<string, string> _ErrorCodes = new Dictionary<string, string>
    {
        { "00000", "提交成功" },
        { "F0001", "参数appkey未填写" },
        { "F0002", "参数appcode未填写" },
        { "F0003", "参数phone未填写" },
        { "F0004", "参数sign未填写" },
        { "F0005", "参数timestamp未填写" },
        { "F0006", "appkey不存在" },
        { "F0007", "账号已经关闭" },
        { "F0008", "sign检验错误" },
        { "F0009", "账号下没有业务" },
        { "F0010", "业务不存在" },
        { "F0011","手机号码超过1000个" },
        {"F0012","timestamp不是数字"},
        {"F0013","timestamp过期超过5分钟"},
        {"F0014","请求ip不在白名单内"},
        {"F0015","余额不足"},
        {"F0016","手机号码无效"},
        {"F0017","没有可用的业务"},
        {"F0022","参数msg未填写"},
        {"F0023","msg超过了1000个字"},
        {"F0024","extend不是纯数字"},
        {"F0025","内容签名未报备/无签名"},
        {"F0039","参数sms未填写"},
        {"F0040","参数sms格式不正确"},
        {"F0041","短信条数超过1000条"},
        {"F0050","无数据"},
        {"F0100","未知错误" }
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<MaiTengClient> _logger;

    public MaiTengClient(
        HttpClient httpClient,
            ILogger<MaiTengClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<SendSMSResponse?> SendSMSAsync(SendSMSRequest request)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var sign = MaiTengHasher.ComputeHash(request.AppKey, request.AppSecret, timestamp);

        var msgTemplate = $"【科技】{request.Msg}";

        var parameter = new Dictionary<string, string>
        {
            { "appkey", request.AppKey },
            { "appcode", request.AppCode },
            { "sign", sign },
            { "phone", request.Phone},
            { "msg", msgTemplate },
            { "timestamp", timestamp.ToString() }
        };

        var content = new StringContent(JsonSerializer.Serialize(parameter), Encoding.UTF8, MediaTypeNames.Application.Json);

        var httpResponseMessage = await _httpClient
            .PostAsync($"/sms/batch/v1", content)
            .ConfigureAwait(false);

        var responseMessage = default(string);

        var jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();

        _logger.LogInformation(responseMessage);

        _ = httpResponseMessage.EnsureSuccessStatusCode();

        var jsonNode = JsonNode.Parse(jsonResponse);

        var code = jsonNode!["code"]!.ToString();

        if (code != "00000")
        {
            var description = jsonNode!["desc"]!.ToString();
            _logger.LogInformation(description);
        }

        var uid = jsonNode!["uid"]!.ToString();

        return new SendSMSResponse
        {
            Uid = uid,
        };
    }
}
