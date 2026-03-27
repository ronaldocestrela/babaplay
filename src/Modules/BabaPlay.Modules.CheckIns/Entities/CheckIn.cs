using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.CheckIns.Entities;

public class CheckIn : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string AssociateId { get; set; } = string.Empty;
    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;

    public CheckInSession? Session { get; set; }
}
