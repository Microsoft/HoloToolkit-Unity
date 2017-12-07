﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/// <summary>
/// Implement this to register your object as a focuser
/// Most focusers will be pointers, but not all pointers will be focusers
/// </summary>
public interface IFocuser
{
    bool InteractionEnabled { get; }

    bool FocusLocked { get; set; }
}