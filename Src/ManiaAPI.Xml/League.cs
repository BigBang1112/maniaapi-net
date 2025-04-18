﻿namespace ManiaAPI.Xml;

public sealed partial record League(string Name, string Path, string LogoUrl)
{
    public string GetFullPath() => string.Join('|', Path, Name);
}
