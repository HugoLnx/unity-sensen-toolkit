using System.Collections.Generic;
using NUnit.Framework;
using SensenToolkit;
using UnityEngine;

public class StringUtilsTests
{

    #region FindMostSimilar
    [Test, Category("FindMostSimilar")]
    public void StringUtils_FindMostSimilar()
    {
        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "apple", "banana", "grape" }, "appl"),
            Is.EqualTo("apple")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "cat", "bat", "rat" }, "cow"),
            Is.EqualTo("cat")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "hello", "hallo", "hullo" }, "hel"),
            Is.EqualTo("hello")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "Keyboard", "Mouse", "Gamepad" }, "key"),
            Is.EqualTo("Keyboard")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "sun", "moon", "star" }, "starry"),
            Is.EqualTo("star")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "KeyboardAndMouse", "Gamepad", "Keyboard" }, "key"),
            Is.EqualTo("Keyboard")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "Keyboard&Mouse", "Key", "Keyboard" }, "keyboard-and-mouse"),
            Is.EqualTo("Keyboard&Mouse")
        );
    }

    [Test, Category("FindMostSimilar")]
    public void StringUtils_FindMostSimilar_EdgeCases()
    {
        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "" }, ""),
            Is.EqualTo("")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "a" }, ""),
            Is.EqualTo("a")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "" }, "a"),
            Is.EqualTo("")
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { }, null),
            Is.EqualTo(null)
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { }, "abc"),
            Is.EqualTo(null)
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { null }, null),
            Is.EqualTo(null)
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { "xpto" }, null),
            Is.EqualTo(null)
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { null, "test" }, null),
            Is.EqualTo(null)
        );

        Assert.That(
            StringUtils.FindMostSimilar(new List<string> { null, "test" }, "test"),
            Is.EqualTo("test")
        );
    }
    #endregion
}
