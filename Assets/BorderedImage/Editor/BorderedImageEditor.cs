using UnityEditor;

namespace BorderedImage.Editor
{
    [CustomEditor(typeof(BorderedImage))]
    public class BorderedImageEditor : UnityEditor.UI.ImageEditor
    {
        private SerializedProperty _borderSize;
        private SerializedProperty _falloffDist;
        private SerializedProperty _borderRadius;

        protected override void OnEnable()
        {
            base.OnEnable();

            _borderSize = serializedObject.FindProperty("_borderSize");
            _falloffDist = serializedObject.FindProperty("_falloffDistance");
            _borderRadius = serializedObject.FindProperty("_borderRadius");

            EditorApplication.update -= UpdateImage;
            EditorApplication.update += UpdateImage;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SpriteGUI();
            EditorGUILayout.PropertyField(m_Color);
            EditorGUILayout.PropertyField(_borderSize);
            EditorGUILayout.PropertyField(_falloffDist);
            EditorGUILayout.PropertyField(_borderRadius, true);
            RaycastControlsGUI();
            EditorGUILayout.Space();
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateImage()
        {
            if (target != null)
            {
                ((BorderedImage) target).Update();
            }
            else
            {
                EditorApplication.update -= UpdateImage;
            }
        }
    }
}
