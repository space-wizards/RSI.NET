using System.Text.Json.Serialization;

namespace SpaceWizards.RsiLib.RSI;

public record struct RsiSize(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y
)
{
    public (int x, int y) CoordinatesForFrame(int index, int fileWidth)
    {
        var x = index * X;
        var y = x / fileWidth * Y;
        x = x % fileWidth;

        return (x, y);
    }
}