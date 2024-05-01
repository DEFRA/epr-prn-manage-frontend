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
    public ActionResult Get(string user)
    {
        PrnHomeViewModel model = new PrnHomeViewModel { OrganisationName = "Tesco", OrganisationNumber = "11244" };
        MaterialData materialData = new MaterialData { MaterialName = "Paper/Board", CurrentBalanace = 500.04, BalanceAwaitingAuthorisation = 200.0, AvialableBalanace = 300.04 };
        model.MaterialDataList.Add(materialData);

        return View(model);
    }

    [HttpGet]
    [Route(PagePaths.PrnDetail)]
    public ActionResult PrnDetail()
    {
        return View();
    }
}