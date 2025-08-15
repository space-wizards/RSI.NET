using SpaceWizards.RsiLib.DMI;
using NUnit.Framework;

namespace SpaceWizards.RsiLib.Tests.DMI;

[TestFixture]
[TestOf(typeof(DmiState))]
public class DmiStateTests
{
    [Test]
    // 3 frames
    [TestCase(2, 5, 2)]
    [TestCase(3, 5, 1)]
    [TestCase(4, 5, 0)]

    // 4 frames
    [TestCase(3, 7, 3)]
    [TestCase(4, 7, 2)]
    [TestCase(5, 7, 1)]
    [TestCase(6, 7, 0)]

    // 5 frames
    [TestCase(4, 9, 4)]
    [TestCase(5, 9, 3)]
    [TestCase(6, 9, 2)]
    [TestCase(7, 9, 1)]
    [TestCase(8, 9, 0)]
    public void GetRewindIndexTest(int currentFrame, int totalFrames, int expectedIndex)
    {
        var rewindIndex = DmiState.GetRewindIndex(currentFrame, totalFrames);
        Assert.That(rewindIndex, Is.EqualTo(expectedIndex));
    }
}
