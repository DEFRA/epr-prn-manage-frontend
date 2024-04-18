﻿namespace PRNPortal.UI.ViewModels;

using System.ComponentModel.DataAnnotations;
using Resources;

public class ConfirmRemovalViewModel
{
    public string OrganisationName { get; set; }

    [Required(ErrorMessageResourceName = "Select_yes_if_you_would_like_to_remove_this_member_from_your_account", ErrorMessageResourceType = typeof(ErrorMessages))]
    public YesNoAnswer? SelectedConfirmRemoval { get; set; }
}