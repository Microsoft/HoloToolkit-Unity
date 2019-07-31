﻿using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExamplesHubHomeButtonActivation : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Home button which brings the user back to examples hub")]
    private GameObject buttonHubHome = null;

    void OnEnable()
    {
        if (MixedRealityToolkit.IsSceneSystemEnabled == true)
        {
            if (buttonHubHome != null)
            {
                buttonHubHome.SetActive(true);
            }
        }
    }
}
