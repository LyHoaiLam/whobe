namespace whobe.DTOs.Report;

public class ReportResponseDto {
    public long ReportId { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = null!;
    public byte TargetType { get; set; }
    public Guid TargetId { get; set; }
    public byte ReasonCode { get; set; }
    public string? Description { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
