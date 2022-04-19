using System.Threading.Tasks;

namespace ATGMedia.GAPShared.SBS.Utility.Helper
{
    public interface ILogHelper
    {
        Task ApplicationLogAsync(AppLogRequest request);

        Task AuditLogAsync(AuditLogRequest request);
    }
}