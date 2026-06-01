using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public static class OverviewAutoSetup
{
    private const string OverviewFolder = "Assets/Scenes/Overviews";

    [MenuItem("Tools/Setup Overview Narration (All Scenes)")]
    public static void AddNarrationToAllOverviews()
    {
        // تجميع جميع ملفات المشهد داخل المجلد المحدد
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { OverviewFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool changed = false;

            // البحث عن أي Canvas أو Panel يمثل شاشة المقدمة
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                // نبحث عن زر "Start" داخل المشهد (اسم عادةً StartButton أو StartBtn)
                Button startBtn = root.GetComponentInChildren<Button>(true);
                if (startBtn == null) continue;

                // نضيف المكوّن إذا لم يكن موجوداً
                OverviewNarration narr = root.GetComponentInChildren<OverviewNarration>(true);
                if (narr == null)
                {
                    // نضيف المكوّن إلى العنصر الرئيسي (عادةً Canvas أو Panel)
                    GameObject target = startBtn.transform.parent?.gameObject ?? startBtn.gameObject;
                    narr = target.AddComponent<OverviewNarration>();
                    changed = true;
                }

                // ربط زر البداية
                narr.startButton = startBtn.gameObject;

                // يمكن للمطور أن يحدد المفتاح يدوياً في الـ Inspector بعد العملية
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"✅ تم إضافة OverviewNarration إلى المشهد: {path}");
            }
            else
            {
                Debug.Log($"ℹ️ لا حاجة لتعديل المشهد: {path}");
            }
        }

        AssetDatabase.Refresh();
    }
}
