using System;

namespace SpaceWizards.RsiLib.Extensions;

public static class FileExtensions
{
    public static bool IsCaseInsensitive()
    {
        return OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();
    }
}
