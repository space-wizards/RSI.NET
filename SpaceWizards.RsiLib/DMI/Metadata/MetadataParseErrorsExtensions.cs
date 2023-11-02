namespace SpaceWizards.RsiLib.DMI.Metadata;

public static class MetadataParseErrorsExtensions
{
    public static ParseError WithMessage(this MetadataErrors error, string message)
    {
        return new ParseError(error, message);
    }
}
