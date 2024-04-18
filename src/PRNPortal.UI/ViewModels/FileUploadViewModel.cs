namespace PRNPortal.UI.ViewModels;

public class FileUploadViewModel : ViewModelWithOrganisationRole
{
    public List<string> ExceptionErrorCodes { get; set; } = new ();
}
