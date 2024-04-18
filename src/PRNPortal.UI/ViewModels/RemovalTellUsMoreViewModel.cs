using System.ComponentModel.DataAnnotations;

namespace PRNPortal.UI.ViewModels;

public class RemovalTellUsMoreViewModel
{
    [MaxLength(200, ErrorMessage = "RemovalTellUsMore.Error200")]
    [Required(ErrorMessage = "RemovalTellUsMore.Error")]
    public string TellUsMore { get; set; }

    public string SelectedReasonForRemoval { get; set; }
}
