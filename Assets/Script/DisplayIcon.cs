using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orignal reference taken from http://www.pinvoke.net/default.aspx/shell32.SHGetFileInfo
/// </summary>
public class DisplayIcon : MonoBehaviour
{
    /// <summary>
    /// Give the path here to exe
    /// </summary>
    public string[] path;

    public GameObject imagePrefab;
    public Transform acttachPoint;

    void Start()
    {
        GenerateExeIcon();
    }
    
    void GenerateExeIcon()
    {
        for(int i = 0; i < path.Length; i++)
        {
            if (path[i] == null || path[i] == "") return;

            GameObject go = Instantiate(imagePrefab);
            go.GetComponent<RawImage>().texture = ExtractIcon.GetTextureFromIconatPath(path[i]);
            go.transform.SetParent(acttachPoint);
        }
    }
}
