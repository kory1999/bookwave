using UnityEditor;
using UnityEngine;

namespace BeWild.AIBook.Editor
{
    public class AIBookEditorMenu
    {
        [MenuItem("BeWild/AIBook/DeleteAllPlayerPrefs")]
        public static void DeleteAllPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}