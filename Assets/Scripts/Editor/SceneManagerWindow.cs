namespace Cass.EditorTools
{
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneManagerWindow : EditorWindow
    {
        [MenuItem("Window/General/Scene Manager")]
        public static void ShowWindow() => GetWindow<SceneManagerWindow>("Scene Manager");

        private void OnGUI()
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(sceneName, GUILayout.MaxWidth(120));
                bool isOpenScene = EditorSceneManager.GetSceneByName(sceneName).name != null;
                if (EditorSceneManager.GetActiveScene().name == sceneName && EditorSceneManager.sceneCount == 1)
                {
                    GUI.backgroundColor = Color.grey;
                }
                if (GUILayout.Button("Open", GUILayout.MaxWidth(120)))
                {
                    if (EditorSceneManager.GetActiveScene().name == sceneName)
                    {
                        Debug.Log("Don't do this please -___-");
                    }
                    else
                    {
                        EditorSceneManager.OpenScene(path);
                    }
                }

                GUI.backgroundColor = Color.white;

                var color = new GUIStyle(GUI.skin.button);

                if (isOpenScene && EditorSceneManager.sceneCount == 1)
                {
                    color.normal.textColor = Color.black;
                    GUI.backgroundColor = Color.grey;
                }

                if (GUILayout.Button(isOpenScene ? "Close Additive" : "Open Additive", color, GUILayout.MaxWidth(120)))
                {
                    if (isOpenScene && EditorSceneManager.sceneCount != 1)
                    {
                        EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByBuildIndex(i), true);
                    }
                    else
                    {
                        EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    }

                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUI.backgroundColor = Color.white;
            }
        }

    }
}
