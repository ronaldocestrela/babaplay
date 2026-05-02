namespace BabaPlay.Domain.Enums;

/// <summary>
/// Lifecycle state of a tenant's isolated database.
/// </summary>
public enum ProvisioningStatus
{
    /// <summary>Job is in the queue and has not started yet.</summary>
    Pending = 0,

    /// <summary>Worker is actively creating the database and running migrations.</summary>
    InProgress = 1,

    /// <summary>Database is ready and the tenant can operate normally.</summary>
    Ready = 2,

    /// <summary>Provisioning failed; manual intervention may be required.</summary>
    Failed = 3,
}
