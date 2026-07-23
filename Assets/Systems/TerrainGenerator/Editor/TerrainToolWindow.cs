/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : TerrainToolWindow.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* View of the terrain editor tool (MVP pattern): an EditorWindow that
* draws the config field, the Generate/Clear buttons and the status
* line, and forwards every click to TerrainToolPresenter. Opened via
* Tools > Terrain Generator.
*
* History :
* 18.07.2026 ER Created
******************************************************************************/

using UnityEngine;
using UnityEditor;

/// <summary>
/// View of the terrain tool (MVP): draws the window, forwards clicks
/// to the presenter and displays its status. Deliberately dumb -
/// every decision lives in the presenter.
/// </summary>
public class TerrainToolWindow : EditorWindow
{
    // Serialized so the assigned config survives window and editor restarts.
    [SerializeField] private TerrainConfig _config;

    private TerrainToolPresenter _presenter = new TerrainToolPresenter();

    /// <summary>
    /// Opens the Terrain Generator window from the Tools menu.
    /// </summary>
    [MenuItem("Tools/Terrain Generator")]
    private static void ShowWindow()
    {
        GetWindow<TerrainToolWindow>("Terrain Generator");
    }

    private void OnGUI()
    {
        // IMGUI fields return their new value, so the field re-assigns itself.
        _config = (TerrainConfig)EditorGUILayout.ObjectField("Config", _config, typeof(TerrainConfig), false);

        if (!_presenter.CanGenerate(_config))
        {
            EditorGUILayout.HelpBox("Assign a TerrainConfig asset.", MessageType.Warning);
        }
        using (new EditorGUI.DisabledScope(!_presenter.CanGenerate(_config)))
        {
            if (GUILayout.Button("Generate Terrain")) _presenter.Generate(_config);
        }

        if (GUILayout.Button("Clear Terrain"))
        {
            _presenter.Clear();
        }
        EditorGUILayout.LabelField(_presenter.StatusMessage);
    }
}