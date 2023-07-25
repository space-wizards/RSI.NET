using System.Collections.Generic;
using System.IO;
using FluentAssertions.Json;
using SpaceWizards.RsiLib.Directions;
using SpaceWizards.RsiLib.RSI;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SpaceWizards.RsiLib.Tests.RSI;

[TestFixture]
[TestOf(typeof(Rsi))]
public class RsiSaveTest
{
    [Test]
    public void EmptyIgnoredTest()
    {
        var state = new RsiState
        {
            Delays = new List<List<float>>(),
            Flags = new Dictionary<string, object>()
        };
        var states = new List<RsiState>() { state };

        Assert.IsEmpty(states[0].Delays);
        Assert.IsEmpty(states[0].Flags);

        var rsi = new Rsi(states: states);
        var stream = new MemoryStream();

        rsi.SaveMetadataToStream(stream);
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();

        Assert.That(json, Is.Not.Empty);
        Assert.That(json, Does.Not.Contain("delays"));
        Assert.That(json, Does.Not.Contain("flags"));
    }

    [Test]
    public void NotEmptySerializedTest()
    {
        var state = new RsiState
        {
            Delays = new List<List<float>> { new() { 5 } },
            Flags = new Dictionary<string, object> { ["Key"] = "Value" }
        };
        var states = new List<RsiState>() { state };

        Assert.IsNotEmpty(states[0].Delays);
        Assert.IsNotEmpty(states[0].Flags);

        var rsi = new Rsi(states: states);
        var stream = new MemoryStream();

        rsi.SaveMetadataToStream(stream);
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();

        Assert.That(json, Is.Not.Empty);

        Assert.That(json, Does.Contain("delays"));
        Assert.That(json, Does.Contain("5"));

        Assert.That(json, Does.Contain("flags"));
        Assert.That(json, Does.Contain("Key"));
        Assert.That(json, Does.Contain("Value"));
    }

    [Test]
    public void ConsistencyTest()
    {
        // TODO: I see no guarantee in the code that multi-key flags dictionaries maintain consistent ordering.
        
        var state = new RsiState
        {
            Name = "RsiState",
            Directions = DirectionType.Diagonal,
            Delays = new List<List<float>> { new() { 5 } },
            Flags = new Dictionary<string, object> { ["Key"] = "Value" },
        };
        var states = new List<RsiState>() { state };

        Assert.IsNotEmpty(states[0].Delays);
        Assert.IsNotEmpty(states[0].Flags);

        var rsi = new Rsi(x: 42, y: 42)
        {
            Version = 5,
            License = "License",
            Copyright = "Copyright",
            States = states
        };

        var stream = new MemoryStream();
        rsi.SaveMetadataToStream(stream);

        var firstJson = new StreamReader(stream).ReadToEnd();
        stream.Position = 0;
        rsi = Rsi.FromMetaJson(stream);

        Assert.That(rsi.Version, Is.EqualTo(5));
        Assert.That(rsi.License, Is.EqualTo("License"));
        Assert.That(rsi.Copyright, Is.EqualTo("Copyright"));
        Assert.That(rsi.Size, Is.EqualTo(new RsiSize(42, 42)));

        Assert.That(rsi.States.Count, Is.EqualTo(1));

        state = rsi.States[0];
        Assert.That(state.Name, Is.EqualTo("RsiState"));
        Assert.That(state.Directions, Is.EqualTo(DirectionType.Diagonal));

        Assert.NotNull(state.Delays);
        Assert.That(state.Delays!.Count, Is.EqualTo(1));
        Assert.That(state.Delays[0].Count, Is.EqualTo(1));
        Assert.That(state.Delays[0][0], Is.EqualTo(5));

        Assert.NotNull(state.Flags);
        Assert.That(state.Flags!.Count, Is.EqualTo(1));
        Assert.That(state.Flags["Key"].ToString(), Is.EqualTo("Value"));

        stream = new MemoryStream();
        rsi.SaveMetadataToStream(stream);

        var secondJson = new StreamReader(stream).ReadToEnd();

        Assert.That(firstJson, Is.EqualTo(secondJson));
    }

    [Test]
    public void MinimalJsonTest()
    {
        var rsi = new Rsi(x: 32, y: 32)
        {
            Version = 5,
            License = "License",
            Copyright = "Copyright",
            States = new List<RsiState>()
            {
                new()
                {
                    Name = "RsiState0",
                    Directions = DirectionType.None,
                    Delays = new List<List<float>>()
                },
                new()
                {
                    Name = "RsiState1",
                    Directions = DirectionType.Cardinal,
                    Delays = new List<List<float>>
                    {
                        new() { 1f },
                        new() { 1f },
                        new() { 1f },
                        new() { 1f },
                    }
                },
                new()
                {
                    Name = "RsiState2",
                    Directions = DirectionType.Cardinal,
                    Delays = new List<List<float>>
                    {
                        new() { 1f, 1f },
                        new() { 1f, 1f },
                        new() { 1f, 1f },
                        new() { 1f, 1f },
                    }
                },
                new()
                {
                    Name = "RsiState3",
                    Directions = DirectionType.Cardinal,
                    Delays = new List<List<float>>
                    {
                        new() { 1f },
                        new() { 2f },
                        new() { 2f },
                        new() { 1f },
                    }
                }
            }
        };

        var stream = new MemoryStream();
        rsi.SaveMetadataToStream(stream);
        stream.Position = 0;

        var json = new StreamReader(stream).ReadToEnd();

        var parse = JToken.Parse(json);

        var states0 = parse.SelectToken("$.states[0]");
        states0.SelectToken("name").Should().HaveValue("RsiState0");
        states0.Should().NotHaveElement("directions");

        var states1 = parse.SelectToken("$.states[1]");
        states1.SelectToken("name").Should().HaveValue("RsiState1");
        states1.Should().HaveElement("directions");
        states1.Should().NotHaveElement("delays");

        var states2 = parse.SelectToken("$.states[2]");
        states2.SelectToken("name").Should().HaveValue("RsiState2");
        states2.Should().HaveElement("directions");
        states2.Should().HaveElement("delays");

        var states3 = parse.SelectToken("$.states[3]");
        states3.SelectToken("name").Should().HaveValue("RsiState3");
        states3.Should().HaveElement("directions");
        states3.Should().HaveElement("delays");
        states3.SelectToken("delays[0]").Should().BeEquivalentTo("[]");
        states3.SelectToken("delays[1]").Should().BeEquivalentTo("[2]");
        states3.SelectToken("delays[2]").Should().BeEquivalentTo("[2]");
        states3.SelectToken("delays[3]").Should().BeEquivalentTo("[]");
    }
}