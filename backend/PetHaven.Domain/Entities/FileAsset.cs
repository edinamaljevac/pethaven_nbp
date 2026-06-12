using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class FileAsset : BaseEntity
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public Guid? UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }
}