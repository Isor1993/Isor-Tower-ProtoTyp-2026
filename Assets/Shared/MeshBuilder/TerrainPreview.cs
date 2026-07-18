/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : TerrainPreview.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Scene-side test driver for MeshBuilder: creates a test heightmap
* (flat, random or ramp), runs Build and assigns the result to the
* local MeshFilter. Not part of the final pipeline - will be replaced
* by the TerrainGenerator orchestrator later.
*
* History :
* 18.07.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// Test MonoBehaviour that feeds MeshBuilder with a hand-made heightmap
/// so the mesh generation can be verified in the scene.
/// </summary>
public class TerrainPreview : MonoBehaviour
{
    [Tooltip("Vertices per edge of the test heightmap (grid is always square).")]
    [SerializeField] private int _heightmapResolution;

    private float[,] _heightmap;

    private void Awake()
    {
        _heightmap = new float[_heightmapResolution, _heightmapResolution];
    }

    private void Start()
    {
        for (int z = 0; z < _heightmap.GetLength(0); z++)
        {
            for (int x = 0; x < _heightmap.GetLength(1); x++)
            {
                // Ramp test: height rises linearly along +X (0 at the first
                // column, 1 at the last). Verifies that heights land at the
                // right grid positions and in the right direction.
                _heightmap[x, z] = x / (_heightmap.GetLength(1) - 1f);
            }
        }

        GetComponent<MeshFilter>().mesh = MeshBuilder.Build(_heightmap, 20, 5);
    }
}
