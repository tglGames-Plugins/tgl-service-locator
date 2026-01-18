using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TGL.ServiceLocator.Editor
{
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ServiceLocatorType))]
	public class ServiceLocatorTypeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Get current enum value
			ServiceLocatorType currentValue = (ServiceLocatorType)property.enumValueIndex;
        
			// Display as label (not editable)
			EditorGUI.LabelField(position, label.text, currentValue.ToString());
		}
	}
#endif
}