using AgitekBackend.DTOs.Reports;

namespace AgitekBackend.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportDTO> GetAsync(ReportQueryDTO query);
    }
}
