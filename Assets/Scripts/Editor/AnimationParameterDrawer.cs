// Editor/AnimationParameterDrawer.cs
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AnimationParameterAttribute))]
public class AnimationParameterDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Obtener el componente que contiene esta propiedad
        var component = property.serializedObject.targetObject as MonoBehaviour;
        if (component == null) return;

        // Obtener el Animator
        var animator = component.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // Obtener el AnimatorController
        var controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller == null) return;

        // Recopilar todas las animaciones disponibles
        var animations = new List<string> { "None" };
        foreach (var layer in controller.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                animations.Add(state.state.name);
            }
        }

        // Encontrar el índice actual
        int currentIndex = animations.IndexOf(property.stringValue);
        if (currentIndex == -1) currentIndex = 0;

        // Mostrar el dropdown
        EditorGUI.BeginProperty(position, label, property);

        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, animations.ToArray());
        if (newIndex != currentIndex)
        {
            property.stringValue = (newIndex == 0) ? "" : animations[newIndex];
        }

        EditorGUI.EndProperty();
    }
}
#endif