using whobe.DTOs.Common;
using whobe.DTOs.Report;

namespace whobe.Interfaces;

public interface IReportService
{
    Task<ReportResponseDto> CreateAsync(Guid reporterId, CreateReportDto dto);
    Task<PagedResultDto<ReportResponseDto>> GetAllAsync(int page, int pageSize, byte? status = null);
    Task ReviewAsync(long reportId, Guid reviewerId, byte newStatus);
}
