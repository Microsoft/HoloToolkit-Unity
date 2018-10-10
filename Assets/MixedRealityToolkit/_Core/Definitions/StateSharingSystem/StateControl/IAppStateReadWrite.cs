﻿using Microsoft.MixedReality.Toolkit.Core.Definitions.StateSharingSystem.Core;
using System;

namespace Microsoft.MixedReality.Toolkit.Core.Definitions.StateSharingSystem.StateControl
{
    /// <summary>
    /// A game state interface control over locking, flushing and adding of scene state objects.
    /// </summary>
    public interface IAppStateReadWrite : IAppStateReadOnly
    {
        /// <summary>
        /// True when server considers states synchronized enough to proceed with app.
        /// </summary>
        new bool Synchronized { get; set; }

        /// <summary>
        /// Adds a state of type to gamestate.
        /// If app state doesn't have an ObjectStateArray of type T an exception will be thrown.
        /// </summary>
        void AddState<T>(T state) where T : struct, IItemState<T>;

        /// <summary>
        /// Adds state of type. Session num must be specified.
        /// If no valid item key is specified, next available item key will be generated.
        /// Returns item key.
        /// </summary>
        sbyte AddStateOfType(Type type, sbyte sessionNum, sbyte stateKey = -1);

        /// <summary>
        /// Sets state using state's item key.
        /// If app state can't find state with item key an exception is thrown.
        /// </summary>
        void SetState<T>(T state) where T : struct, IItemState<T>;

        /// <summary>
        /// Flushes all object state arrays.
        /// </summary>
        void Flush();

        /// <summary>
        /// Flushes object state array of type T.
        /// </summary>
        void Flush<T>() where T : struct, IItemState<T>;
    }
}