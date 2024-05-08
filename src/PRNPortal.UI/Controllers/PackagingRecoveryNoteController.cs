using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PRNPortal.Application.Constants;
using PRNPortal.Application.Options;
using PRNPortal.UI.ViewModels;

namespace PRNPortal.UI.Controllers;

public class PackagingRecyclingNoteController : Controller
{

    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<PackagingRecyclingNoteController> _logger;

    public PackagingRecyclingNoteController(
        ILogger<PackagingRecyclingNoteController> logger,
        IAuthorizationService authorizationService,
        IOptions<GlobalVariables> globalVariables)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    //TODO: Uncomment once we hook up with real API
    //[HttpGet]
    //[Route(PagePaths.PrnView)]
    //public ActionResult Get()
    //{
    //    return View();
    //}

    [HttpGet]
    [Route(PagePaths.PrnView)]
    public ActionResult Get(string user = "1")
    {
        PrnHomeViewModel model = new PrnHomeViewModel {SiteDataList= new List<SiteData>()};

        if (user == "1") // 1 item
        {
            MaterialData materialData = new MaterialData { MaterialName = "Wood", CurrentBalanace = 500.04, BalanceAwaitingAuthorisation = 200.0, AvialableBalanace = 300.04 };
            SiteData siteData = new SiteData { OrganisationName = "Tesco", OrganisationNumber = "11244", SiteInfo = "156 Hamilton Road" };

            siteData.MaterialDataList.Add(materialData);
            model.SiteDataList.Add(siteData);

        }
        else if (user == "2")
        {
            SiteData siteData = new SiteData { OrganisationName = "Argos", OrganisationNumber = "565343", SiteInfo = "12 Ruby Street" };

            MaterialData materialData1 = new MaterialData { MaterialName = "Plastic", CurrentBalanace = 400.05, BalanceAwaitingAuthorisation = 200.0, AvialableBalanace = 200.05 };
            siteData.MaterialDataList.Add(materialData1);

            MaterialData materialData2 = new MaterialData { MaterialName = "Steel", CurrentBalanace = 800.0, BalanceAwaitingAuthorisation = 500.0, AvialableBalanace = 300.0 };
            siteData.MaterialDataList.Add(materialData2);

            MaterialData materialData3 = new MaterialData { MaterialName = "Steel", CurrentBalanace = 100.0, BalanceAwaitingAuthorisation = 100.0, AvialableBalanace = 0.0, IsAvialableBalanace = false };


            siteData.MaterialDataList.Add(materialData3);
            model.SiteDataList.Add(siteData);
        }
        else if (user == "3")
        {
            SiteData siteData = new SiteData { OrganisationName = "Argos", OrganisationNumber = "565343", SiteInfo = "12 Ruby Street" };

            SiteData siteData2 = new SiteData { OrganisationName = "Amazon", OrganisationNumber = "956675", SiteInfo = "8 Russel Street" };

            MaterialData materialData2 = new MaterialData { MaterialName = "Steel", CurrentBalanace = 800.0, BalanceAwaitingAuthorisation = 500.0, AvialableBalanace = 300.0 };
            siteData.MaterialDataList.Add(materialData2);

            MaterialData materialData3 = new MaterialData { MaterialName = "Steel", CurrentBalanace = 100.0, BalanceAwaitingAuthorisation = 100.0, AvialableBalanace = 0.0, IsAvialableBalanace = false };
            siteData2.MaterialDataList.Add(materialData3);

            model.SiteDataList.Add(siteData);
            model.SiteDataList.Add(siteData2);

        }
        else //  no materials
        {
            SiteData siteData = new SiteData { OrganisationName = "Amazon", OrganisationNumber = "956675", SiteInfo = "8 Russel Street" };
            model.SiteDataList.Add(siteData);
        }

        
        return View(model);
    }

    [HttpGet]
    [Route(PagePaths.PrnDetail)]
    public ActionResult PrnDetail()
    {
        return View();
    }
}