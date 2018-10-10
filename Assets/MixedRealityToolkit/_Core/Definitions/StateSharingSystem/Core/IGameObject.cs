﻿using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Definitions.StateSharingSystem.Core
{
    #region base interfaces

    /// <summary>
    /// Interface for objects that inherit from MonoBehavior (or to simulate such an object).
    /// </summary>
    public interface IGameObject
    {
        string name { get; set; }
        Transform transform { get; }
        GameObject gameObject { get; }
    }

    #endregion
}