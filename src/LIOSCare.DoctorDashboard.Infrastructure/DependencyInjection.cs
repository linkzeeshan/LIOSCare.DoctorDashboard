using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Infrastructure.Http;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using LIOSCare.DoctorDashboard.Infrastructure.Security;
using LIOSCare.DoctorDashboard.Infrastructure.Services;
using LIOSCare.DoctorDashboard.Infrastructure.Services.Email;
using Microsoft.Extensions.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace LIOSCare.DoctorDashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDoctorDashboardInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<MessagingServiceOptions>(configuration.GetSection("MessagingService"));
        services.Configure<NotificationServiceOptions>(configuration.GetSection("NotificationService"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.Configure<PasswordResetOptions>(configuration.GetSection("PasswordReset"));
        services.Configure<QuickChatOptions>(configuration.GetSection("QuickChat"));

        if (string.IsNullOrWhiteSpace(configuration["Email:SmtpHost"]))
            services.AddScoped<IEmailService, LoggingEmailService>();
        else
            services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddDbContext<DoctorPortalDbContext>(options =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("SocialPlatformDb"),
                    npgsql => npgsql.MigrationsHistoryTable(
                        "__ef_migrations_history_doctor_dashboard",
                        "provider"
                    )
                )
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
        );

        services.AddScoped<PasswordHashingService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDashboardQueryService, DashboardQueryService>();
        services.AddScoped<IDoctorProfileService, DoctorProfileService>();
        services.AddScoped<IQuickChatService, QuickChatService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ISessionReportService, SessionReportService>();
        services.AddScoped<IServiceTierService, ServiceTierService>();
        services.AddHttpClient<IMessagingServiceClient, HttpMessagingServiceClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<MessagingServiceOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddHttpClient<INotificationClient, HttpNotificationClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<NotificationServiceOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<DoctorDashboardSeeder>();

        return services;
    }
}