using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

public class ModelAnimationHelper : EditorWindow
{
    private GameObject modelPrefab;
    private Vector2 scrollPos;
    private string controllerName = "FantasmaController";

    // Esto crea el menú en la barra superior de Unity
    [MenuItem("Window/Animation/Model Animation Helper")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ModelAnimationHelper), false, "Animation Helper");
    }

    void OnGUI()
    {
        GUILayout.Label("Model Animation Helper", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Campo para el modelo
        modelPrefab = (GameObject)EditorGUILayout.ObjectField("Model Prefab", modelPrefab, typeof(GameObject), false);

        // Campo para el nombre del controller
        controllerName = EditorGUILayout.TextField("Controller Name", controllerName);

        EditorGUILayout.Space();

        if (modelPrefab != null)
        {
            string modelPath = AssetDatabase.GetAssetPath(modelPrefab);

            // Mostrar todas las animaciones encontradas
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            bool foundAnimations = false;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var clip in clips)
            {
                if (clip is AnimationClip)
                {
                    foundAnimations = true;
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(clip.name);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = clip;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            if (!foundAnimations)
            {
                EditorGUILayout.HelpBox("No animations found in this model.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Create Animator Controller"))
                {
                    CreateAnimatorController(clips);
                }
            }
        }
    }

    private void CreateAnimatorController(Object[] clips)
    {
        // Crear el directorio si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        // Crear el controller
        string controllerPath = $"Assets/Animations/{controllerName}.controller";

        // Crear el animator controller
        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Obtener la capa root
        var rootStateMachine = controller.layers[0].stateMachine;

        // Añadir un estado idle por defecto
        var idleState = rootStateMachine.AddState("Idle");
        rootStateMachine.defaultState = idleState;

        // Añadir estados para cada clip
        foreach (var obj in clips)
        {
            if (obj is AnimationClip clip)
            {
                // No procesar clips que empiecen con "__" ya que suelen ser metadatos
                if (clip.name.StartsWith("__")) continue;

                var state = rootStateMachine.AddState(clip.name);
                state.motion = clip;

                // Crear un parámetro trigger para esta animación
                string triggerName = clip.name.Replace(" ", "_").Replace("|", "_");
                controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);

                // Crear una transición desde cualquier estado
                var transition = rootStateMachine.AddAnyStateTransition(state);
                transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                transition.duration = 0.25f; // Duración de la transición
                transition.hasExitTime = false;

                // Crear una transición de vuelta al idle
                var returnTransition = state.AddTransition(idleState);
                returnTransition.hasExitTime = true;
                returnTransition.exitTime = 0.9f; // Transición cerca del final de la animación
                returnTransition.duration = 0.1f;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Animator Controller created at {controllerPath}");

        // Seleccionar el controller creado
        Selection.activeObject = controller;

        // Mostrar en el Project window
        EditorGUIUtility.PingObject(controller);

        // Intentar asignar el controller al prefab si tiene un Animator
        var animator = modelPrefab.GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(animator);
        }
    }
}