namespace PRNPortal.UI.ViewModels;

public class PrnHomeViewModel
{
    public string? OrganisationName { get; set; }

    public string? OrganisationNumber { get; set; }

   public List<MaterialData> MaterialDataList { get; set; } =  new List<MaterialData>();

    public string? SiteInfo { get; set; }   

}

//TODO: Should be removed later            
public class MaterialData
{
    public string? MaterialName { get; set; }
    public string? MaterialId { get; set;}      
    public string? MaterialType { get; set;}
    public double? CurrentBalanace  { get; set;}
    public double? BalanceAwaitingAuthorisation  { get; set;}
    public double? AvialableBalanace { get; set; }
}