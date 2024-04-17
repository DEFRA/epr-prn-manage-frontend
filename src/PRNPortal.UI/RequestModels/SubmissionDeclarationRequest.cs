﻿namespace PRNPortal.UI.RequestModels;

using System.ComponentModel.DataAnnotations;
using Resources;

public class SubmissionDeclarationRequest
{
    [Required(ErrorMessageResourceName = "declaration_name_required_key", ErrorMessageResourceType = typeof(ErrorMessages))]
    [MaxLength(200, ErrorMessageResourceName = "declaration_name_max_length_key", ErrorMessageResourceType = typeof(ErrorMessages))]
    public string DeclarationName { get; set; }
}