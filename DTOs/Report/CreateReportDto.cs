namespace whobe.DTOs.Report;

public class CreateReportDto {
    public byte TargetType { get; set; }
    public Guid TargetId { get; set; }
    public byte ReasonCode { get; set; }
    public string? Description { get; set; }
}
