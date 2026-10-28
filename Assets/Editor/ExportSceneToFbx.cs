using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public class ExportSceneToFbx
{
    [MenuItem("Tools/Export TrailerScene")]
    public static void Export()
    {
        var fbxPath = @"e:\UnityProjects\ostatnie-echo\TRAILER\TrailerScene.fbx";
        var objectsToExport = new System.Collections.Generic.HashSet<GameObject>();
        var renderers = Object.FindObjectsOfType<MeshRenderer>();
        foreach(var r in renderers) {
            Transform t = r.transform;
            while(t != null) {
                objectsToExport.Add(t.gameObject);
                t = t.parent;
            }
        }

        // export objects using fbx api
        var options = new UnityEditor.Formats.Fbx.Exporter.ExportModelOptions {
            ExportFormat = UnityEditor.Formats.Fbx.Exporter.ExportFormat.Binary
        };
        
        var objectsArray = new System.Collections.Generic.List<GameObject>(objectsToExport).ToArray();
        UnityEditor.Formats.Fbx.Exporter.ModelExporter.ExportObjects(fbxPath, objectsArray, options);
        
        Debug.Log("Exported TrailerScene to " + fbxPath);
    }
}
#endif
