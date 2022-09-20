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

        while(transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

 }
