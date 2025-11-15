namespace crm_backend.Modules.Marketing;

public class CampaignMember
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    
    // Member can be a Lead, Customer, or Contact
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    public int? CustomerId { get; set; }
    public Customer.Customer? Customer { get; set; }
    
    public int? ContactId { get; set; }
    public Customer.Contact? Contact { get; set; }
    
    // Status Tracking
    public CampaignMemberStatus Status { get; set; } = CampaignMemberStatus.Sent;
    public DateTime? RespondedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
}

public enum CampaignMemberStatus
{
    Sent,
    Opened,
    Clicked,
    Responded,
    Converted,
    Unsubscribed,
    Bounced
}

