using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixMouseDrivenDoorHinges
{
    private const string RequestPath = "Assets/Editor/FixMouseDrivenDoorHinges.request";
    private const string ReportPath = "Assets/Editor/FixMouseDrivenDoorHinges.report.txt";
    private const string DoorScriptGuid = "0d1cd11f3a5c45938e7da1e28ea93262";
    private const string HingeName = "DoorHinge";

    [InitializeOnLoadMethod]
    private static void RunWhenRequested()
    {
        if (!File.Exists(RequestPath))
        {
            return;
        }

        EditorApplication.delayCall += RunRequestedFix;
    }

    [MenuItem("Tools/Environment/Fix Mouse Driven Door Hinges")]
    public static void FixAllLoadedSceneDoorsFromMenu()
    {
        FixAllLoadedSceneDoors();
    }

    public static void RunRequestedFix()
    {
        try
        {
            FixAllScenesContainingDoors();

            if (File.Exists(RequestPath))
            {
                File.Delete(RequestPath);
            }
        }
        catch (Exception exception)
        {
            File.WriteAllText(ReportPath, "Door hinge fix failed:\n" + exception);
            throw;
        }
        finally
        {
            AssetDatabase.Refresh();
        }
    }

    public static void FixAllScenesContainingDoors()
    {
        string[] scenePaths = FindScenePathsContainingDoorScript();
        StringBuilder combinedReport = new StringBuilder();
        combinedReport.AppendLine("Fix MouseDrivenDoor Hinges");
        combinedReport.AppendLine("Run time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        combinedReport.AppendLine("Scenes containing MouseDrivenDoor: " + scenePaths.Length);
        combinedReport.AppendLine();

        for (int i = 0; i < scenePaths.Length; i++)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePaths[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            SceneReport sceneReport = FixSceneDoors(scene);

            combinedReport.AppendLine("Scene: " + scene.path);
            combinedReport.AppendLine("Doors found: " + sceneReport.DoorsFound);
            combinedReport.AppendLine("Fixed doors: " + sceneReport.FixedDoors);
            combinedReport.AppendLine("Already valid doors: " + sceneReport.AlreadyValidDoors);
            combinedReport.AppendLine("Verified valid doors: " + sceneReport.VerifiedValidDoors + " / " + sceneReport.DoorsFound);

            if (sceneReport.Messages.Count > 0)
            {
                combinedReport.AppendLine("Warnings:");
                foreach (string message in sceneReport.Messages)
                {
                    combinedReport.AppendLine("- " + message);
                }
            }

            if (sceneReport.Actions.Count > 0)
            {
                combinedReport.AppendLine("Actions:");
                foreach (string action in sceneReport.Actions)
                {
                    combinedReport.AppendLine("- " + action);
                }
            }

            combinedReport.AppendLine();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        File.WriteAllText(ReportPath, combinedReport.ToString());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void FixAllLoadedSceneDoors()
    {
        StringBuilder combinedReport = new StringBuilder();
        combinedReport.AppendLine("Fix MouseDrivenDoor Hinges");
        combinedReport.AppendLine("Run time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        combinedReport.AppendLine();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                continue;
            }

            SceneReport sceneReport = FixSceneDoors(scene);

            combinedReport.AppendLine("Scene: " + scene.path);
            combinedReport.AppendLine("Doors found: " + sceneReport.DoorsFound);
            combinedReport.AppendLine("Fixed doors: " + sceneReport.FixedDoors);
            combinedReport.AppendLine("Already valid doors: " + sceneReport.AlreadyValidDoors);
            combinedReport.AppendLine("Verified valid doors: " + sceneReport.VerifiedValidDoors + " / " + sceneReport.DoorsFound);

            if (sceneReport.Messages.Count > 0)
            {
                combinedReport.AppendLine("Warnings:");
                foreach (string message in sceneReport.Messages)
                {
                    combinedReport.AppendLine("- " + message);
                }
            }

            if (sceneReport.Actions.Count > 0)
            {
                combinedReport.AppendLine("Actions:");
                foreach (string action in sceneReport.Actions)
                {
                    combinedReport.AppendLine("- " + action);
                }
            }

            combinedReport.AppendLine();
            EditorSceneManager.SaveScene(scene);
        }

        File.WriteAllText(ReportPath, combinedReport.ToString());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static SceneReport FixSceneDoors(Scene scene)
    {
        List<MouseDrivenDoor> doors = FindSceneDoors(scene);
        UnpackPrefabRootsContainingDoors(doors);
        RemoveEmptyDoorHinges(scene);

        doors = FindSceneDoors(scene);
        doors.Sort((a, b) => string.Compare(GetHierarchyPath(a.transform), GetHierarchyPath(b.transform), StringComparison.Ordinal));

        SceneReport report = new SceneReport
        {
            DoorsFound = doors.Count
        };

        foreach (MouseDrivenDoor door in doors)
        {
            Transform doorTransform = door.transform;
            SerializedObject serializedDoor = new SerializedObject(door);
            SerializedProperty hingeRootProperty = serializedDoor.FindProperty("hingeRoot");

            if (IsValidPrivateHinge(doorTransform, hingeRootProperty))
            {
                report.AlreadyValidDoors++;
                report.Actions.Add("Already valid: " + GetHierarchyPath(doorTransform));
                continue;
            }

            Transform oldParent = doorTransform.parent;
            Transform sceneParent = oldParent != null && oldParent.name == HingeName && oldParent.childCount == 1
                ? oldParent.parent
                : oldParent;
            int oldSiblingIndex = sceneParent != null
                ? (oldParent != null && oldParent.name == HingeName ? oldParent.GetSiblingIndex() : doorTransform.GetSiblingIndex())
                : -1;
            Vector3 hingePosition = CalculateHingePosition(door, serializedDoor);

            GameObject hinge = FindReusableHinge(door, hingeRootProperty, oldParent);
            if (hinge == null)
            {
                hinge = new GameObject(HingeName);
            }

            if (hinge.transform.parent != sceneParent)
            {
                hinge.transform.SetParent(sceneParent, true);
            }

            if (sceneParent == null)
            {
                SceneManager.MoveGameObjectToScene(hinge, doorTransform.gameObject.scene);
            }

            hinge.transform.SetPositionAndRotation(hingePosition, doorTransform.rotation);
            hinge.transform.localScale = Vector3.one;

            doorTransform.SetParent(hinge.transform, true);
            if (oldSiblingIndex >= 0)
            {
                hinge.transform.SetSiblingIndex(oldSiblingIndex);
            }

            hingeRootProperty.objectReferenceValue = hinge.transform;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.RecordPrefabInstancePropertyModifications(doorTransform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hinge.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(door);

            EditorSceneManager.MarkSceneDirty(doorTransform.gameObject.scene);

            report.FixedDoors++;
            report.Actions.Add("Fixed: " + doorTransform.name + " -> " + GetHierarchyPath(hinge.transform));
        }

        RemoveEmptyDoorHinges(scene);
        VerificationResult verification = VerifyDoorHinges(doors);

        report.VerifiedValidDoors = verification.ValidCount;
        report.Messages.AddRange(verification.Messages);

        return report;
    }

    private static string[] FindScenePathsContainingDoorScript()
    {
        List<string> scenePaths = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in guids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
            {
                continue;
            }

            string sceneText = File.ReadAllText(scenePath);
            if (sceneText.Contains(DoorScriptGuid))
            {
                scenePaths.Add(scenePath);
            }
        }

        scenePaths.Sort(StringComparer.Ordinal);
        return scenePaths.ToArray();
    }

    private static void UnpackPrefabRootsContainingDoors(List<MouseDrivenDoor> doors)
    {
        HashSet<GameObject> roots = new HashSet<GameObject>();

        foreach (MouseDrivenDoor door in doors)
        {
            if (door == null || !PrefabUtility.IsPartOfPrefabInstance(door.gameObject))
            {
                continue;
            }

            GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(door.gameObject);
            if (root != null)
            {
                roots.Add(root);
            }
        }

        foreach (GameObject root in roots)
        {
            if (root != null && PrefabUtility.IsPartOfPrefabInstance(root))
            {
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }
    }

    private static void RemoveEmptyDoorHinges(Scene scene)
    {
        List<GameObject> emptyHinges = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            CollectEmptyDoorHinges(root.transform, emptyHinges);
        }

        for (int i = emptyHinges.Count - 1; i >= 0; i--)
        {
            if (emptyHinges[i] != null)
            {
                UnityEngine.Object.DestroyImmediate(emptyHinges[i]);
            }
        }
    }

    private static void CollectEmptyDoorHinges(Transform root, List<GameObject> emptyHinges)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            CollectEmptyDoorHinges(root.GetChild(i), emptyHinges);
        }

        if (root.name == HingeName && root.childCount == 0)
        {
            emptyHinges.Add(root.gameObject);
        }
    }

    private static GameObject FindReusableHinge(MouseDrivenDoor door, SerializedProperty hingeRootProperty, Transform oldParent)
    {
        if (oldParent != null
            && oldParent.name == HingeName
            && oldParent.childCount == 1
            && oldParent.GetChild(0) == door.transform)
        {
            return oldParent.gameObject;
        }

        Transform serializedHinge = hingeRootProperty != null ? hingeRootProperty.objectReferenceValue as Transform : null;
        if (serializedHinge != null && serializedHinge.name == HingeName && serializedHinge.childCount == 0)
        {
            return serializedHinge.gameObject;
        }

        return null;
    }

    private static List<MouseDrivenDoor> FindSceneDoors(Scene targetScene)
    {
        MouseDrivenDoor[] allDoors = Resources.FindObjectsOfTypeAll<MouseDrivenDoor>();
        List<MouseDrivenDoor> sceneDoors = new List<MouseDrivenDoor>();

        foreach (MouseDrivenDoor door in allDoors)
        {
            if (door == null || EditorUtility.IsPersistent(door.gameObject))
            {
                continue;
            }

            Scene scene = door.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded || scene != targetScene)
            {
                continue;
            }

            sceneDoors.Add(door);
        }

        return sceneDoors;
    }

    private static bool IsValidPrivateHinge(Transform doorTransform, SerializedProperty hingeRootProperty)
    {
        Transform parent = doorTransform.parent;
        Transform hingeRoot = hingeRootProperty != null ? hingeRootProperty.objectReferenceValue as Transform : null;

        return parent != null
            && parent.name == HingeName
            && parent.childCount == 1
            && parent.GetChild(0) == doorTransform
            && hingeRoot == parent;
    }

    private static Vector3 CalculateHingePosition(MouseDrivenDoor door, SerializedObject serializedDoor)
    {
        Transform doorTransform = door.transform;
        Vector3 center = TryGetCombinedBounds(doorTransform, out Bounds bounds) ? bounds.center : doorTransform.position;
        Vector3 axis = doorTransform.right.sqrMagnitude > 0f ? doorTransform.right.normalized : Vector3.right;
        float halfWidth = TryGetCombinedBounds(doorTransform, out bounds)
            ? GetHalfExtentAlongAxis(bounds, axis)
            : Mathf.Max(0.25f, Mathf.Abs(doorTransform.lossyScale.x) * 0.5f);

        SerializedProperty hingeSideProperty = serializedDoor.FindProperty("hingeSide");
        bool rightHinge = hingeSideProperty != null && hingeSideProperty.enumValueIndex == 1;
        float sideSign = rightHinge ? 1f : -1f;

        return center + axis * halfWidth * sideSign;
    }

    private static bool TryGetCombinedBounds(Transform root, out Bounds combinedBounds)
    {
        bool hasBounds = false;
        combinedBounds = new Bounds(root.position, Vector3.zero);

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        if (hasBounds)
        {
            return true;
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }

    private static float GetHalfExtentAlongAxis(Bounds bounds, Vector3 axis)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                    float projected = Vector3.Dot(corner, axis);
                    min = Mathf.Min(min, projected);
                    max = Mathf.Max(max, projected);
                }
            }
        }

        return Mathf.Max(0.01f, (max - min) * 0.5f);
    }

    private static VerificationResult VerifyDoorHinges(List<MouseDrivenDoor> doors)
    {
        VerificationResult result = new VerificationResult();
        HashSet<Transform> usedHinges = new HashSet<Transform>();

        foreach (MouseDrivenDoor door in doors)
        {
            SerializedObject serializedDoor = new SerializedObject(door);
            SerializedProperty hingeRootProperty = serializedDoor.FindProperty("hingeRoot");
            Transform hingeRoot = hingeRootProperty != null ? hingeRootProperty.objectReferenceValue as Transform : null;
            Transform parent = door.transform.parent;

            if (hingeRoot == null)
            {
                result.Messages.Add(GetHierarchyPath(door.transform) + " has no HingeRoot assigned.");
                continue;
            }

            if (hingeRoot != parent)
            {
                result.Messages.Add(GetHierarchyPath(door.transform) + " HingeRoot is not its direct parent.");
                continue;
            }

            if (hingeRoot.name != HingeName)
            {
                result.Messages.Add(GetHierarchyPath(door.transform) + " HingeRoot is not named DoorHinge.");
                continue;
            }

            if (hingeRoot.childCount != 1 || hingeRoot.GetChild(0) != door.transform)
            {
                result.Messages.Add(GetHierarchyPath(door.transform) + " DoorHinge has extra children.");
                continue;
            }

            if (!usedHinges.Add(hingeRoot))
            {
                result.Messages.Add(GetHierarchyPath(door.transform) + " shares a DoorHinge with another door.");
                continue;
            }

            result.ValidCount++;
        }

        return result;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return "<null>";
        }

        List<string> names = new List<string>();
        Transform current = transform;

        while (current != null)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();
        return string.Join("/", names);
    }

    private sealed class VerificationResult
    {
        public int ValidCount;
        public readonly List<string> Messages = new List<string>();
    }

    private sealed class SceneReport
    {
        public int DoorsFound;
        public int FixedDoors;
        public int AlreadyValidDoors;
        public int VerifiedValidDoors;
        public readonly List<string> Actions = new List<string>();
        public readonly List<string> Messages = new List<string>();
    }
}
