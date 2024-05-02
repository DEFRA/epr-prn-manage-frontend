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
        PrnHomeViewModel model = new PrnHomeViewModel { MaterialDataList = new List<MaterialData>() };

        if (user == "1") // 1 item
        {
            model = new PrnHomeViewModel { OrganisationName = "Tesco", OrganisationNumber = "11244", SiteInfo= "156 Hamilton Road" };
            MaterialData materialData = new MaterialData { MaterialName = "Wood", CurrentBalanace = 500.04, BalanceAwaitingAuthorisation = 200.0, AvialableBalanace = 300.04 };
            model.MaterialDataList.Add(materialData);
        }
        else if (user == "2") 
        {
            model = new PrnHomeViewModel { OrganisationName = "Argos", OrganisationNumber = "565343", SiteInfo = "12 Ruby Street" };
            MaterialData materialData1 = new MaterialData { MaterialName = "Plastic", CurrentBalanace = 400.05, BalanceAwaitingAuthorisation = 200.0, AvialableBalanace = 200.05 };
            model.MaterialDataList.Add(materialData1);
            MaterialData materialData2 = new MaterialData { MaterialName = "Steel", CurrentBalanace = 800.0, BalanceAwaitingAuthorisation = 500.0, AvialableBalanace = 300.0 };
            model.MaterialDataList.Add(materialData2);
        }  
        else //  no materials
        {
            model = new PrnHomeViewModel { OrganisationName = "Amazon", OrganisationNumber = "956675", SiteInfo = "8 Russel Street" };
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