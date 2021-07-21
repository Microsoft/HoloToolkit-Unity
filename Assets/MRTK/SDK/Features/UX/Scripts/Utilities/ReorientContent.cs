using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReorientContent : MonoBehaviour
{
    // Start is called before the first frame update
    public void AdjustContentOrientation()
    {
        MixedRealitySceneContent sceneContent = GameObject.Find("MixedRealitySceneContent").GetComponent<MixedRealitySceneContent>();
        sceneContent.ReorientContent();
    }
}
