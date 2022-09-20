using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUtils : MonoBehaviour
{
    public void ResetChildren()
    {

        if(transform.childCount == 0)
        {
            Debug.Log("No children to delete");
            return;
        }

        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void OnValidate()
    {
        ResetChildren();
    }
}
