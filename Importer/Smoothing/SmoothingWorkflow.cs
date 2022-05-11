using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

public static class SmoothingWorkflow
{
    public static Rsi Transform(Rsi input, BaseSmoothingInstance importer, BaseSmoothingInstance exporter)
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

        var outputSubstates = exporter.TilesetToSubstates(tiles);
        var outputSubstateIdx = 0;

        var outputStates = new List<RsiState>();
        for (var i = 0; i < exporter.SourceStateNameSuffixes.Length; i++)
        {
            // this stays null for now, but scribble it down for later
            List<List<float>>? delays = null;
            var state = new RsiState(exporter.SourceStateNameSuffixes[i], exporter.SourceDirectionType, delays, null, null, null);
            // Pull substates into states, properly grouping them with directions
            for (var j = 0; j < (int) exporter.SourceDirectionType; j++)
                state.Frames[j, 0] = outputSubstates[outputSubstateIdx++];
            outputStates.Add(state);
        }
        return new Rsi(Rsi.CurrentRsiVersion, exporter.RsiSize, outputStates, input.License, input.Copyright);
    }
}

