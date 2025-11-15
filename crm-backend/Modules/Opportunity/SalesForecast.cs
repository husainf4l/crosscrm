namespace crm_backend.Modules.Opportunity;

/// <summary>
/// Sales Forecast for a specific period (Month, Quarter, Year)
/// </summary>
public class SalesForecast
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ForecastPeriod Period { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    // Forecast Amounts
    public decimal ForecastAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance => ActualAmount - ForecastAmount;
    
    // Forecast By (User, Team, Territory)
    public ForecastByType ForecastByType { get; set; }
    public int? ForecastByUserId { get; set; }
    public User.User? ForecastByUser { get; set; }
    
    public int? ForecastByTeamId { get; set; }
    public Collaboration.Team? ForecastByTeam { get; set; }
    
    public int? ForecastByTerritoryId { get; set; }
    public Territory? ForecastByTerritory { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastCalculatedAt { get; set; }
    
    // Navigation
    public ICollection<ForecastItem> Items { get; set; } = new List<ForecastItem>();
}

public enum ForecastPeriod
{
    Month,
    Quarter,
    Year,
    Custom
}

public enum ForecastByType
{
    User,
    Team,
    Territory,
    Company
}

/// <summary>
/// ForecastItem links an Opportunity to a SalesForecast
/// </summary>
public class ForecastItem
{
    public int Id { get; set; }
    public int SalesForecastId { get; set; }
    public SalesForecast SalesForecast { get; set; } = null!;
    
    public int OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = null!;
    
    // Forecasted values
    public decimal ForecastedAmount { get; set; }
    public decimal Probability { get; set; } // 0-100
    public decimal WeightedAmount => ForecastedAmount * (Probability / 100);
    
    // Actual values (updated when opportunity closes)
    public decimal? ActualAmount { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

