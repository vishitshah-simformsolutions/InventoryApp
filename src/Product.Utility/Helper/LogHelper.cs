using ATGMedia.Core.Logger.AppLog;
using ATGMedia.Core.Logger.AuditLog;
using ATGMedia.Core.Logger.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ATGMedia.GAPShared.SBS.Utility.Helper
{
    [ExcludeFromCodeCoverage]
    public class AuditLogRequest
    {
        public string MethodName { get; set; }
        public Actions Action { get; set; }
        public LogLevels LogLevel { get; set; }
        public string Message { get; set; }
        public string CorrelationId { get; set; }
        public dynamic AdditionalDetails { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AppLogRequest
    {
        public string MethodName { get; set; }
        public Actions Action { get; set; }
        public LogLevels LogLevel { get; set; }
        public string Message { get; set; }
        public string CorrelationId { get; set; }
        public dynamic AdditionalDetails { get; set; }
    }

    /// <summary>
    /// Log helper handles all types of logging throughout the application.
    /// ATG.Logging package should not be directly used anywhere in the code directly.
    /// Always use log helper for logging purpose.
    /// You can add more methods here if required.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogHelper : ILogHelper
    {
        private readonly string _applicationName;
        private readonly string _applicationType;
        private readonly string _applicationVersion;
        private readonly Regions _sourceRegion;

        private readonly IAppLog _appLogger;
        private readonly IAuditLog _auditLogger;

        public LogHelper(IAppLog appLogger, IAuditLog auditLogger, IConfiguration configuration)
        {
            _applicationName = configuration["ServiceName"];
            _applicationVersion = configuration["Version"];
            _applicationType = configuration["ApplicationType"];
            _sourceRegion = (Regions)Enum.Parse(typeof(Regions), configuration["ApplicationRegion"] ?? string.Empty);

            _appLogger = appLogger;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// Used to insert application level logs.
        /// </summary>
        /// <param name="request">App log request</param>
        public async Task ApplicationLogAsync(AppLogRequest request)
        {
            var logModel = new AppLogValueModel
            {
                LogReference = new LogReferenceModel
                {
                    CorrelationId = request.CorrelationId,
                    Source = _applicationName,
                    SourceRegion = _sourceRegion,
                    ApplicationType = _applicationType,
                    ClientIpAddress = GetLocalIpAddress(),
                    ApplicationVersion = _applicationVersion
                },
                Context = request.MethodName,
                LogLevel = request.LogLevel,
                Action = request.Action,
                AdditionalDetails = request.AdditionalDetails
            };

            await _appLogger.LogAsync(logModel, request.Message);
        }

        /// <summary>
        /// Used to insert audit logs for all actions performed.
        /// </summary>
        /// <param name="request">Audit log request</param>
        public async Task AuditLogAsync(AuditLogRequest request)
        {
            var logModel = new AuditLogValueModel
            {
                LogReference = new LogReferenceModel
                {
                    CorrelationId = request.CorrelationId,
                    Source = _applicationName,
                    SourceRegion = _sourceRegion,
                    ApplicationType = _applicationType,
                    ClientIpAddress = GetLocalIpAddress(),
                    ApplicationVersion = _applicationVersion
                },
                Context = request.MethodName,
                LogLevel = request.LogLevel,
                Action = request.Action,
                AdditionalDetails = request.AdditionalDetails
            };

            await _auditLogger.LogAsync(logModel, request.Message ?? $"Audit log from {_applicationType} - {_applicationName} - {_sourceRegion} - {_applicationVersion}.");
        }

        /// <summary>
        /// Returns local ip address for logging.
        /// </summary>
        /// <returns>Ip address string</returns>
        private string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }
    }
}