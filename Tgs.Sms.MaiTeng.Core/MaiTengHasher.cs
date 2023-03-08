using System.Security.Cryptography;
using System.Text;

namespace Tgs.Sms.MaiTeng;

public class MaiTengHasher
{
    private static readonly MD5 _Md5;

    static MaiTengHasher()
    {
        _Md5 = MD5.Create();
    }

    public static string ComputeHash(string AppKey, string AppSecret, long TimeStamp)
    {
        var plainText = $"{AppKey}{AppSecret}{TimeStamp}";

        var data = Encoding.UTF8.GetBytes(plainText);

        var hash = _Md5.ComputeHash(data);

        return Convert.ToHexString(hash).ToLower();
    }
}