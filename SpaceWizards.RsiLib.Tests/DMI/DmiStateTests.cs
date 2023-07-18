using SpaceWizards.RsiLib.DMI;
using NUnit.Framework;

namespace SpaceWizards.RsiLib.Tests.DMI;

[TestFixture]
[TestOf(typeof(DmiState))]
public class DmiStateTests
{
    [Test]
    // 3 frames
    [TestCase(3, 3, 1)]
    [TestCase(4, 3, 0)]

    // 4 frames
    [TestCase(4, 4, 2)]
    [TestCase(5, 4, 1)]

    // 5 frames
    [TestCase(5, 5, 3)]
    [TestCase(6, 5, 2)]
    [TestCase(7, 5, 1)]
    public void GetRewindIndexTest(int currentFrame, int totalFrames, int expectedIndex)
    {
        var rewindIndex = DmiState.GetRewindIndex(currentFrame, totalFrames);
        Assert.That(rewindIndex, Is.EqualTo(expectedIndex));
    }
}