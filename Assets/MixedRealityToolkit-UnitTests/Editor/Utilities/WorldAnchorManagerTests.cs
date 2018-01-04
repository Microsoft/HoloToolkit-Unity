﻿using MixedRealityToolkit.Common;
using NUnit.Framework;
using UnityEngine;

namespace MixedRealityToolkit.Tests.Utilities
{
    public class WorldAnchorManagerTests
    {
        [SetUp]
        public void ClearScene()
        {
            TestUtils.ClearScene();
        }

        [Test]
        public void TestGenerateAnchorNameFromGameObject()
        {
            const string expected = "AnchorName";
            var gameObject = new GameObject(expected);
            var result = WorldAnchorManager.GenerateAnchorName(gameObject);
            Assert.That(result, NUnit.Framework.Is.EqualTo(expected));
        }

        [Test]
        public void TestGenerateAnchorNameFromParameter()
        {
            const string expected = "AnchorName";
            var gameObject = new GameObject();
            var result = WorldAnchorManager.GenerateAnchorName(gameObject, expected);
            Assert.That(result, NUnit.Framework.Is.EqualTo(expected));
        }
    }
}