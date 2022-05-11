using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

public static class SmoothingWorkflow
{
    public static Rsi Transform(Rsi input, string _importPrefix, BaseSmoothingInstance importer, string exportPrefix, string fullState, BaseSmoothingInstance exporter)
    {
        Image<Rgba32>[] importSubstates = new Image<Rgba32>[importer.SubstateCount];
        for (var i = 0; i < importSubstates.Length; i++)
        {
            var stateIdx = importer.SubstateToState(i);
            var direction = importer.SubstateToDirection(i);
            var state = input.States[stateIdx];
            importSubstates[i] = state.FirstFrameFor(direction)!;
        }

        Tileset tiles = importer.SubstatesToTileset(importSubstates);

        var output = OutputTileset(tiles, exportPrefix, fullState, exporter);
        output.License = input.License;
        output.Copyright = input.Copyright;
        return output;
    }

    private static Rsi OutputTileset(Tileset tileset, string exportPrefix, string fullState, BaseSmoothingInstance exporter)
    {
        var outputSubstates = exporter.TilesetToSubstates(tileset);
        var outputSubstateIdx = 0;

        var outputStates = new List<RsiState>();
        for (var i = 0; i < exporter.SourceStateNameSuffixes.Length; i++)
        {
            // this stays null for now, but scribble it down for later
            List<List<float>>? delays = null;
            var state = new RsiState(exportPrefix + exporter.SourceStateNameSuffixes[i], exporter.SourceDirectionType, delays, null, null, null);
            // Pull substates into states, properly grouping them with directions
            for (var j = 0; j < (int) exporter.SourceDirectionType; j++)
                state.Frames[j, 0] = outputSubstates[outputSubstateIdx++];
            outputStates.Add(state);
        }
        // If applicable, add full state
        var fullImage = tileset[0];
        if ((exporter.RsiSize.X == fullImage.Width) && (exporter.RsiSize.Y == fullImage.Height))
        {
            var state = new RsiState(fullState, DirectionType.None, null, null, null, null);
            state.Frames[0, 0] = fullImage;
            outputStates.Add(state);
        }
        return new Rsi(Rsi.CurrentRsiVersion, exporter.RsiSize, outputStates);
    }

    public static Rsi Transform(Image<Rgba32> input, BaseSmoothingInstance importer, string exportPrefix, string fullState, BaseSmoothingInstance exporter)
    {
        Image<Rgba32>[] importSubstates = {input};

        Tileset tiles = importer.SubstatesToTileset(importSubstates);
        return OutputTileset(tiles, exportPrefix, fullState, exporter);
    }
}

