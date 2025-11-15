namespace crm_backend.Modules.Collaboration;

public class Channel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Channel Configuration
    public ChannelType Type { get; set; } = ChannelType.Public;
    public bool IsArchived { get; set; } = false;
    
    // Relationships
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    
    public int? CustomerId { get; set; }
    public Customer.Customer? Customer { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<ChannelMember> Members { get; set; } = new List<ChannelMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public enum ChannelType
{
    Public,
    Private,
    Direct,
    Customer
}

