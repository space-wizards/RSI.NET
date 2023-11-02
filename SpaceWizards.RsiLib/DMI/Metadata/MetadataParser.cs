using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MetadataExtractor.Formats.Png;
using static SpaceWizards.RsiLib.DMI.Metadata.MetadataErrors;

namespace SpaceWizards.RsiLib.DMI.Metadata;

public class MetadataParser
{
    private const string Header = "BEGIN DMI";

    private bool TryGetFileDmiTag(Stream stream, [NotNullWhen(true)] out RawMetadata? rawData)
    {
        var data = PngMetadataReader.ReadMetadata(stream);

        foreach (var datum in data)
        {
            foreach (var tag in datum.Tags)
            {
                var hasTags = tag.Description?.Contains(Header) ?? false;

                if (hasTags)
                {
                    rawData = new RawMetadata(tag.Description!);
                    return true;
                }
            }
        }

        rawData = null;
        return false;
    }

    public bool TryGetFileMetadata(
        Stream stream,
        [NotNullWhen(true)] out IMetadata? metadata,
        [NotNullWhen(false)] out ParseError? error)
    {
        if (!TryGetFileDmiTag(stream, out var raw))
        {
            metadata = null;
            error = NoDmiTag.WithMessage("No dmi tag found");
            return false;
        }

        if (!raw.Next() || !raw.TryVersion(out var version))
        {
            metadata = null;
            error = NoVersion.WithMessage("No version found");
            return false;
        }

        var states = new List<DmiState>();

        do
        {
            if (raw.TryState(out var state))
            {
                states.Add(state);
            }
        } while (raw.Next());

        metadata = new Metadata(version, states);
        error = null;
        return true;
    }
}
