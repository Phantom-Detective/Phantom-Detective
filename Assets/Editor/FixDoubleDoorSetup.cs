using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixDoubleDoorSetup
{
    private const string RequestPath = "Assets/Editor/FixDoubleDoorSetup.request";
    private const string ReportPath = "Assets/Editor/FixDoubleDoorSetup.report.txt";
    private const string LeftHingeName = "DoorHinge_Left";
    private const string RightHingeName = "DoorHinge_Right";

    [InitializeOnLoadMethod]
    private static void RunWhenRequested()
    {
        if (!File.Exists(RequestPath))
        {
            return;
        }

        EditorApplication.delayCall += RunRequestedFix;
    }

    private static void RunRequestedFix()
    {
        Run();

        if (File.Exists(RequestPath))
        {
            File.Delete(RequestPath);
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Tools/Environment/Fix Double Door Setup")]
    public static void Run()
    {
        string[] scenePaths = FindScenesContainingDoubleDoors();
        StringBuilder report = new StringBuilder();
        report.AppendLine("Fix Double Door Setup");
        report.AppendLine("Run time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        report.AppendLine("Scenes containing double doors: " + scenePaths.Length);
        report.AppendLine();

        int totalLeaves = 0;
        int totalGroups = 0;
        int totalNonLeafDoorScriptsRemoved = 0;
        int totalCollidersAdjusted = 0;

        for (int i = 0; i < scenePaths.Length; i++)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePaths[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            SceneReport sceneReport = FixScene(scene);

            totalLeaves += sceneReport.LeavesFixed;
            totalGroups += sceneReport.GroupsFixed;
            totalNonLeafDoorScriptsRemoved += sceneReport.NonLeafDoorScriptsRemoved;
            totalCollidersAdjusted += sceneReport.CollidersAdjusted;

            report.AppendLine("Scene: " + scene.path);
            report.AppendLine("Double door groups fixed: " + sceneReport.GroupsFixed);
            report.AppendLine("Double door leaves fixed: " + sceneReport.LeavesFixed);
            report.AppendLine("Non-leaf MouseDrivenDoor components removed: " + sceneReport.NonLeafDoorScriptsRemoved);
            report.AppendLine("Parent/frame colliders adjusted: " + sceneReport.CollidersAdjusted);
            report.AppendLine("Verified leaves: " + sceneReport.VerifiedLeaves + " / " + sceneReport.LeavesFixed);

            if (sceneReport.Warnings.Count > 0)
            {
                report.AppendLine("Warnings:");
                foreach (string warning in sceneReport.Warnings)
                {
                    report.AppendLine("- " + warning);
                }
            }

            if (sceneReport.Actions.Count > 0)
            {
                report.AppendLine("Actions:");
                foreach (string action in sceneReport.Actions)
                {
                    report.AppendLine("- " + action);
                }
            }

            report.AppendLine();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        report.AppendLine("Totals:");
        report.AppendLine("Double door groups fixed: " + totalGroups);
        report.AppendLine("Double door leaves fixed: " + totalLeaves);
        report.AppendLine("Non-leaf MouseDrivenDoor components removed: " + totalNonLeafDoorScriptsRemoved);
        report.AppendLine("Parent/frame colliders adjusted: " + totalCollidersAdjusted);

        File.WriteAllText(ReportPath, report.ToString());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static SceneReport FixScene(Scene scene)
    {
        SceneReport report = new SceneReport();
        List<Transform> leaves = FindDoubleDoorLeaves(scene);
        HashSet<Transform> groupRoots = new HashSet<Transform>();

        foreach (Transform leaf in leaves)
        {
            Transform groupRoot = FindDoubleDoorGroupRoot(leaf);
            if (groupRoot != null)
            {
                groupRoots.Add(groupRoot);
            }
            else
            {
                report.Warnings.Add("Could not find double door group root for " + GetHierarchyPath(leaf));
            }
        }

        foreach (Transform groupRoot in groupRoots)
        {
            UnwrapGroupRootFromSharedHinge(groupRoot, report);
            report.NonLeafDoorScriptsRemoved += RemoveNonLeafDoorScripts(groupRoot);
            FlattenNonLeafGenericHinges(groupRoot, report);
            report.CollidersAdjusted += FixBlockingParentAndFrameColliders(groupRoot, report);
            report.GroupsFixed++;
        }

        leaves = FindDoubleDoorLeaves(scene);
        foreach (Transform leaf in leaves)
        {
            Transform groupRoot = FindDoubleDoorGroupRoot(leaf);
            if (groupRoot == null)
            {
                continue;
            }

            FixLeafHinge(leaf, groupRoot, report);
            report.LeavesFixed++;
        }

        VerifyDoubleDoorLeaves(scene, report);
        return report;
    }

    private static string[] FindScenesContainingDoubleDoors()
    {
        List<string> scenePaths = new List<string>();
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
            {
                continue;
            }

            string text = File.ReadAllText(scenePath);
            if (text.Contains("DoorD_V2_Left") || text.Contains("DoorD_V2_Right") || text.Contains("DoorD_Left") || text.Contains("DoorD_Right"))
            {
                scenePaths.Add(scenePath);
            }
        }

        scenePaths.Sort(StringComparer.Ordinal);
        return scenePaths.ToArray();
    }

    private static List<Transform> FindDoubleDoorLeaves(Scene scene)
    {
        List<Transform> leaves = new List<Transform>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            CollectDoubleDoorLeaves(root.transform, leaves);
        }

        leaves.Sort((a, b) => string.Compare(GetHierarchyPath(a), GetHierarchyPath(b), StringComparison.Ordinal));
        return leaves;
    }

    private static void CollectDoubleDoorLeaves(Transform root, List<Transform> leaves)
    {
        if (IsDoubleDoorLeaf(root.name))
        {
            leaves.Add(root);
        }

        for (int i = 0; i < root.childCount; i++)
        {
            CollectDoubleDoorLeaves(root.GetChild(i), leaves);
        }
    }

    private static Transform FindDoubleDoorGroupRoot(Transform leaf)
    {
        Transform current = leaf.parent;
        while (current != null)
        {
            if (IsDoubleDoorGroupRoot(current.name))
            {
                return current;
            }

            current = current.parent;
        }

        return leaf.parent != null && leaf.parent.parent != null ? leaf.parent.parent : leaf.parent;
    }

    private static bool IsDoubleDoorLeaf(string objectName)
    {
        return objectName.StartsWith("DoorD_V2_Left", StringComparison.Ordinal)
            || objectName.StartsWith("DoorD_V2_Right", StringComparison.Ordinal)
            || objectName.StartsWith("DoorD_Left", StringComparison.Ordinal)
            || objectName.StartsWith("DoorD_Right", StringComparison.Ordinal);
    }

    private static bool IsLeftLeaf(string objectName)
    {
        return objectName.Contains("_Left") || objectName.EndsWith("D_Left", StringComparison.Ordinal);
    }

    private static bool IsRightLeaf(string objectName)
    {
        return objectName.Contains("_Right") || objectName.EndsWith("D_Right", StringComparison.Ordinal);
    }

    private static bool IsDoubleDoorGroupRoot(string objectName)
    {
        return objectName.StartsWith("DoorD_V2", StringComparison.Ordinal)
            || objectName.StartsWith("DoorD_V1", StringComparison.Ordinal);
    }

    private static void UnwrapGroupRootFromSharedHinge(Transform groupRoot, SceneReport report)
    {
        Transform parent = groupRoot.parent;
        if (parent == null || !parent.name.StartsWith("DoorHinge", StringComparison.Ordinal) || parent.childCount != 1)
        {
            return;
        }

        Transform newParent = parent.parent;
        int siblingIndex = parent.GetSiblingIndex();
        groupRoot.SetParent(newParent, true);
        groupRoot.SetSiblingIndex(siblingIndex);
        UnityEngine.Object.DestroyImmediate(parent.gameObject);
        report.Actions.Add("Unwrapped double door group from shared hinge: " + GetHierarchyPath(groupRoot));
    }

    private static int RemoveNonLeafDoorScripts(Transform groupRoot)
    {
        int removed = 0;
        MouseDrivenDoor[] doors = groupRoot.GetComponentsInChildren<MouseDrivenDoor>(true);

        foreach (MouseDrivenDoor door in doors)
        {
            if (door == null || IsDoubleDoorLeaf(door.name))
            {
                continue;
            }

            UnityEngine.Object.DestroyImmediate(door);
            removed++;
        }

        return removed;
    }

    private static void FlattenNonLeafGenericHinges(Transform groupRoot, SceneReport report)
    {
        List<Transform> genericHinges = new List<Transform>();
        CollectGenericDoorHinges(groupRoot, genericHinges);

        for (int i = genericHinges.Count - 1; i >= 0; i--)
        {
            Transform hinge = genericHinges[i];
            if (hinge == null || hinge.name != "DoorHinge")
            {
                continue;
            }

            bool containsDoubleDoorLeaf = false;
            for (int childIndex = 0; childIndex < hinge.childCount; childIndex++)
            {
                if (IsDoubleDoorLeaf(hinge.GetChild(childIndex).name))
                {
                    containsDoubleDoorLeaf = true;
                    break;
                }
            }

            if (containsDoubleDoorLeaf)
            {
                continue;
            }

            List<Transform> children = new List<Transform>();
            for (int childIndex = 0; childIndex < hinge.childCount; childIndex++)
            {
                children.Add(hinge.GetChild(childIndex));
            }

            foreach (Transform child in children)
            {
                child.SetParent(groupRoot, true);
            }

            report.Actions.Add("Removed non-leaf generic DoorHinge: " + GetHierarchyPath(hinge));
            UnityEngine.Object.DestroyImmediate(hinge.gameObject);
        }
    }

    private static void CollectGenericDoorHinges(Transform root, List<Transform> genericHinges)
    {
        if (root.name == "DoorHinge")
        {
            genericHinges.Add(root);
        }

        for (int i = 0; i < root.childCount; i++)
        {
            CollectGenericDoorHinges(root.GetChild(i), genericHinges);
        }
    }

    private static int FixBlockingParentAndFrameColliders(Transform groupRoot, SceneReport report)
    {
        int adjusted = 0;
        Collider[] colliders = groupRoot.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
        {
            if (collider == null || collider.isTrigger || IsDoubleDoorLeaf(collider.name))
            {
                continue;
            }

            bool isParentOrFrame = collider.transform == groupRoot || collider.name.IndexOf("Frame", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isRelevantCollider = collider is BoxCollider || collider is MeshCollider;
            if (!isParentOrFrame || !isRelevantCollider)
            {
                continue;
            }

            collider.isTrigger = true;
            adjusted++;
            report.Actions.Add("Set parent/frame collider to trigger: " + GetHierarchyPath(collider.transform));
        }

        return adjusted;
    }

    private static void FixLeafHinge(Transform leaf, Transform groupRoot, SceneReport report)
    {
        bool isRight = IsRightLeaf(leaf.name);
        string hingeName = isRight ? RightHingeName : LeftHingeName;
        Transform hinge = FindOrCreateLeafHinge(leaf, groupRoot, hingeName);

        MoveExtraHingeChildrenToGroupRoot(hinge, leaf, groupRoot);

        hinge.name = hingeName;
        hinge.SetParent(groupRoot, true);
        hinge.SetPositionAndRotation(CalculateLeafHingePosition(leaf, isRight), leaf.rotation);
        hinge.localScale = Vector3.one;

        leaf.SetParent(hinge, true);

        MouseDrivenDoor door = leaf.GetComponent<MouseDrivenDoor>();
        if (door == null)
        {
            door = leaf.gameObject.AddComponent<MouseDrivenDoor>();
        }

        SerializedObject serializedDoor = new SerializedObject(door);
        SerializedProperty hingeRootProperty = serializedDoor.FindProperty("hingeRoot");
        SerializedProperty hingeSideProperty = serializedDoor.FindProperty("hingeSide");

        if (hingeRootProperty != null)
        {
            hingeRootProperty.objectReferenceValue = hinge;
        }

        if (hingeSideProperty != null)
        {
            hingeSideProperty.enumValueIndex = isRight ? 1 : 0;
        }

        serializedDoor.ApplyModifiedPropertiesWithoutUndo();
        report.Actions.Add("Fixed " + leaf.name + " -> " + GetHierarchyPath(hinge));
    }

    private static Transform FindOrCreateLeafHinge(Transform leaf, Transform groupRoot, string hingeName)
    {
        if (leaf.parent != null && leaf.parent.name.StartsWith("DoorHinge", StringComparison.Ordinal) && leaf.parent.childCount == 1)
        {
            return leaf.parent;
        }

        Transform existing = groupRoot.Find(hingeName);
        if (existing != null && existing.childCount == 0)
        {
            return existing;
        }

        GameObject hinge = new GameObject(hingeName);
        hinge.transform.SetParent(groupRoot, true);
        return hinge.transform;
    }

    private static void MoveExtraHingeChildrenToGroupRoot(Transform hinge, Transform leaf, Transform groupRoot)
    {
        List<Transform> extraChildren = new List<Transform>();
        for (int i = 0; i < hinge.childCount; i++)
        {
            Transform child = hinge.GetChild(i);
            if (child != leaf)
            {
                extraChildren.Add(child);
            }
        }

        foreach (Transform child in extraChildren)
        {
            child.SetParent(groupRoot, true);
        }
    }

    private static Vector3 CalculateLeafHingePosition(Transform leaf, bool isRight)
    {
        Vector3 center = TryGetCombinedBounds(leaf, out Bounds bounds) ? bounds.center : leaf.position;
        Vector3 axis = leaf.right.sqrMagnitude > 0f ? leaf.right.normalized : Vector3.right;
        float halfWidth = TryGetCombinedBounds(leaf, out bounds)
            ? GetHalfExtentAlongAxis(bounds, axis)
            : Mathf.Max(0.25f, Mathf.Abs(leaf.lossyScale.x) * 0.5f);

        return center + axis * halfWidth * (isRight ? 1f : -1f);
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

    private static void VerifyDoubleDoorLeaves(Scene scene, SceneReport report)
    {
        Dictionary<Transform, Transform> usedHinges = new Dictionary<Transform, Transform>();
        List<Transform> leaves = FindDoubleDoorLeaves(scene);

        foreach (Transform leaf in leaves)
        {
            MouseDrivenDoor door = leaf.GetComponent<MouseDrivenDoor>();
            if (door == null)
            {
                report.Warnings.Add("Missing MouseDrivenDoor on double door leaf: " + GetHierarchyPath(leaf));
                continue;
            }

            SerializedObject serializedDoor = new SerializedObject(door);
            Transform hingeRoot = serializedDoor.FindProperty("hingeRoot")?.objectReferenceValue as Transform;
            Transform parent = leaf.parent;
            string expectedHingeName = IsRightLeaf(leaf.name) ? RightHingeName : LeftHingeName;

            if (parent == null || parent != hingeRoot)
            {
                report.Warnings.Add("Leaf does not use its direct parent as HingeRoot: " + GetHierarchyPath(leaf));
                continue;
            }

            if (parent.name != expectedHingeName)
            {
                report.Warnings.Add("Leaf hinge has wrong name: " + GetHierarchyPath(parent));
                continue;
            }

            if (parent.childCount != 1 || parent.GetChild(0) != leaf)
            {
                report.Warnings.Add("Leaf hinge has extra children: " + GetHierarchyPath(parent));
                continue;
            }

            if (usedHinges.TryGetValue(parent, out Transform otherLeaf))
            {
                report.Warnings.Add("Shared hinge detected between " + GetHierarchyPath(otherLeaf) + " and " + GetHierarchyPath(leaf));
                continue;
            }

            usedHinges.Add(parent, leaf);
            report.VerifiedLeaves++;
        }
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

    private sealed class SceneReport
    {
        public int GroupsFixed;
        public int LeavesFixed;
        public int VerifiedLeaves;
        public int NonLeafDoorScriptsRemoved;
        public int CollidersAdjusted;
        public readonly List<string> Actions = new List<string>();
        public readonly List<string> Warnings = new List<string>();
    }
}
