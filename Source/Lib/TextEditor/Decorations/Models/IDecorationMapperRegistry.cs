﻿namespace Walk.TextEditor.RazorLib.Decorations.Models;

public interface IDecorationMapperRegistry
{
    public IDecorationMapper GetDecorationMapper(string extensionNoPeriod);
}
