﻿namespace PRNPortal.UI.ViewModels;

public class FileReUploadConfirmationViewModel
{
    public Guid SubmissionId { get; set; }

    public string FileName { get; set; }

    public DateTime FileUploadDate { get; set; }

    public DateTime SubmissionDeadline { get; set; }
}