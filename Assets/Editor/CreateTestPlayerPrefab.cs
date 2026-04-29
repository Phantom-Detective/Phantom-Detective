using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateTestPlayerPrefab
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PrefabPath = PrefabFolder + "/TestPlayer.prefab";
    private const string RequestPath = "Assets/Editor/CreateTestPlayerPrefab.request";

    [InitializeOnLoadMethod]
    private static void CreateWhenRequested()
    {
        if (!File.Exists(RequestPath))
        {
            return;
        }

        EditorApplication.delayCall += Create;
    }

    public static void Create()
    {
        EnsureFolder(PrefabFolder);

        GameObject testPlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        testPlayer.name = "TestPlayer";
        testPlayer.transform.position = new Vector3(0f, 1f, -3f);
        testPlayer.transform.rotation = Quaternion.identity;

        RemoveExtraColliders(testPlayer);

        CharacterController controller = testPlayer.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.stepOffset = 0.3f;
        controller.slopeLimit = 45f;

        TestPlayerController playerController = testPlayer.AddComponent<TestPlayerController>();

        GameObject cameraHolder = new GameObject("CameraHolder");
        cameraHolder.transform.SetParent(testPlayer.transform, false);
        cameraHolder.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        cameraHolder.transform.localRotation = Quaternion.identity;

        GameObject mainCamera = new GameObject("MainCamera");
        mainCamera.transform.SetParent(cameraHolder.transform, false);
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;
        mainCamera.tag = "MainCamera";

        Camera camera = mainCamera.AddComponent<Camera>();
        camera.nearClipPlane = 0.03f;
        camera.fieldOfView = 70f;

        MouseLook mouseLook = mainCamera.AddComponent<MouseLook>();

        AssignObjectReference(playerController, "characterController", controller);
        AssignObjectReference(playerController, "cameraHolder", cameraHolder.transform);
        AssignObjectReference(mouseLook, "playerBody", testPlayer.transform);

        PrefabUtility.SaveAsPrefabAsset(testPlayer, PrefabPath);
        Object.DestroyImmediate(testPlayer);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        if (File.Exists(RequestPath))
        {
            AssetDatabase.DeleteAsset(RequestPath);
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string folder = Path.GetFileName(folderPath);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, folder);
    }

    private static void RemoveExtraColliders(GameObject gameObject)
    {
        Collider[] colliders = gameObject.GetComponents<Collider>();
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    private static void AssignObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
