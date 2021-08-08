using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Importer.Directions;
using Importer.RSI;
using NUnit.Framework;

namespace Importer.Tests.RSI
{
    [TestFixture]
    [TestOf(typeof(Rsi))]
    public class RsiSaveTest
    {
        [Test]
        public async Task EmptyIgnoredTest()
        {
            var state = new RsiState
            {
                Delays = new List<List<float>>(),
                Flags = new Dictionary<string, object>()
            };
            var states = new List<RsiState>() {state};

            Assert.IsEmpty(states[0].Delays);
            Assert.IsEmpty(states[0].Flags);

            var rsi = new Rsi(states: states);
            var stream = new MemoryStream();

            await rsi.SaveRsiToStream(stream);
            var json = await new StreamReader(stream).ReadToEndAsync();
            await stream.DisposeAsync();

            Assert.That(json, Is.Not.Empty);
            Assert.That(json, Does.Not.Contain("delays"));
            Assert.That(json, Does.Not.Contain("flags"));
        }

        [Test]
        public async Task NotEmptySerializedTest()
        {
            var state = new RsiState
            {
                Delays = new List<List<float>> {new() {5}},
                Flags = new Dictionary<string, object> {["Key"] = "Value"}
            };
            var states = new List<RsiState>() {state};

            Assert.IsNotEmpty(states[0].Delays);
            Assert.IsNotEmpty(states[0].Flags);

            var rsi = new Rsi(states: states);
            var stream = new MemoryStream();

            await rsi.SaveRsiToStream(stream);
            var json = await new StreamReader(stream).ReadToEndAsync();
            await stream.DisposeAsync();

            Assert.That(json, Is.Not.Empty);

            Assert.That(json, Does.Contain("delays"));
            Assert.That(json, Does.Contain("5"));

            Assert.That(json, Does.Contain("flags"));
            Assert.That(json, Does.Contain("Key"));
            Assert.That(json, Does.Contain("Value"));
        }

        [Test]
        public async Task ConsistencyTest()
        {
            var state = new RsiState
            {
                Name = "RsiState",
                Directions = DirectionType.Diagonal,
                Delays = new List<List<float>> {new() {5}},
                Flags = new Dictionary<string, object> {["Key"] = "Value"},
            };
            var states = new List<RsiState>() {state};

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
            await rsi.SaveRsiToStream(stream);

            var firstJson = await new StreamReader(stream).ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);
            rsi = await Rsi.FromMetaJson(stream);

            await stream.DisposeAsync();

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
            await rsi.SaveRsiToStream(stream);

            var secondJson = await new StreamReader(stream).ReadToEndAsync();
            await stream.DisposeAsync();

            Assert.That(firstJson, Is.EqualTo(secondJson));
        }
    }
}
