﻿namespace Walk.Ide.RazorLib.FileSystems.Models;

public interface IFileTemplateProvider
{
    public List<IFileTemplate> FileTemplatesList { get; }
}