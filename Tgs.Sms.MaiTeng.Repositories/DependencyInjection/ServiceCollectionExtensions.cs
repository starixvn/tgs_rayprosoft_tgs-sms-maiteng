using Microsoft.EntityFrameworkCore;
using Tgs.Sms.MaiTeng;
using Tgs.Sms.MaiTeng.Repositories;
using Tgs.Sms.MaiTeng.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaiTengRepositories(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextConfigure)
        => services
            .AddDbContext<TgsContext>(dbContextConfigure)
            .AddTransient<ITgsRepository, TgsRepository>();
}