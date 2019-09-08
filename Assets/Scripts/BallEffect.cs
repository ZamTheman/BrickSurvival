using UnityEngine;

public class BallEffect : MonoBehaviour
{
    int uvAnimationTileX = 4;
    int uvAnimationTileY = 1;
    float framesPerSecond = 60;

    void Update()
    {
        int index = (int)(Time.time * framesPerSecond);

        index = index % (uvAnimationTileX * uvAnimationTileY);

        var size = new Vector2(1f / uvAnimationTileX, 1f / uvAnimationTileY);

        var uIndex = index % uvAnimationTileX;
        var vIndex = index / uvAnimationTileX;

        var offset = new Vector2(uIndex * size.x, 1f - size.y - vIndex * size.y);

        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
        GetComponent<Renderer>().material.SetTextureScale("_MainTex", size);
    }
}
