namespace crm_backend.Modules.Collaboration;

/// <summary>
/// Approval Process defines the workflow for approving entities (Quote, Contract, etc.)
/// </summary>
public class ApprovalProcess
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EntityType { get; set; } = string.Empty; // "Quote", "Contract", "Invoice", etc.

    // Approval Type
    public ApprovalType ApprovalType { get; set; } = ApprovalType.Sequential; // Sequential or Parallel

    // Conditions (when to trigger this process)
    public string? TriggerConditions { get; set; } // JSON: {"minAmount": 10000, "status": "Sent"}

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Status
    public bool IsActive { get; set; } = true;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
    public ICollection<ApprovalRequest> Requests { get; set; } = new List<ApprovalRequest>();
}

public enum ApprovalType
{
    Sequential, // Steps must be approved in order
    Parallel    // All steps can be approved simultaneously
}

/// <summary>
/// Approval Step defines a single step in the approval process
/// </summary>
public class ApprovalStep
{
    public int Id { get; set; }
    public int ApprovalProcessId { get; set; }
    public ApprovalProcess ApprovalProcess { get; set; } = null!;

    public int StepOrder { get; set; } // Order of this step (1, 2, 3, ...)
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Approver Configuration
    public ApproverType ApproverType { get; set; } = ApproverType.Role;
    public string? ApproverRole { get; set; } // Role name if ApproverType is Role
    public int? ApproverUserId { get; set; } // User ID if ApproverType is User
    public User.User? ApproverUser { get; set; }

    // Step Requirements
    public bool IsRequired { get; set; } = true; // Must be approved to proceed
    public bool CanDelegate { get; set; } = false; // Approver can delegate to another user
    public bool CanSkip { get; set; } = false; // Step can be skipped

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum ApproverType
{
    Role,      // Approve by role
    User,      // Specific user
    Team,      // Any member of team
    Manager    // Manager of requester
}

/// <summary>
/// Approval Request represents a request for approval of a specific entity
/// </summary>
public class ApprovalRequest
{
    public int Id { get; set; }
    public int ApprovalProcessId { get; set; }
    public ApprovalProcess ApprovalProcess { get; set; } = null!;

    public string EntityType { get; set; } = string.Empty; // "Quote", "Contract", etc.
    public int EntityId { get; set; } // ID of the entity being approved

    // Request Details
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public int CurrentStep { get; set; } = 1; // Current step in the process

    public int RequestedByUserId { get; set; }
    public User.User RequestedByUser { get; set; } = null!;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public string? Comments { get; set; } // Comments from requester

    // Completion
    public DateTime? CompletedAt { get; set; }
    public int? CompletedByUserId { get; set; }
    public User.User? CompletedByUser { get; set; }

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<ApprovalResponse> Responses { get; set; } = new List<ApprovalResponse>();
}

public enum ApprovalStatus
{
    Pending,
    InProgress,
    Approved,
    Rejected,
    Cancelled,
    Delegated
}

/// <summary>
/// Approval Response represents a response to an approval step
/// </summary>
public class ApprovalResponse
{
    public int Id { get; set; }
    public int ApprovalRequestId { get; set; }
    public ApprovalRequest ApprovalRequest { get; set; } = null!;

    public int Step { get; set; } // Step number this response is for
    public bool IsApproved { get; set; } // true = Approved, false = Rejected

    public string? Comments { get; set; } // Comments from approver

    public int RespondedByUserId { get; set; }
    public User.User RespondedByUser { get; set; } = null!;
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

    // Delegation (if approver delegated)
    public int? DelegatedToUserId { get; set; }
    public User.User? DelegatedToUser { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

