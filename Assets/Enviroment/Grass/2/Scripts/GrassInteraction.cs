using UnityEngine;

public class GrassInteraction : MonoBehaviour
{

  

    private void Update()
    {       
        Shader.SetGlobalVector("_InteractPos", transform.position);
    }
}
