using UnityEngine;

// A small bobbing marker floating above an object, visible from a distance —
// separate from the close-range "[E] Осмотреть" prompt, which only shows once
// the player is already standing next to it.
public class InteractionHintIcon : MonoBehaviour
{
    public float bobAmplitude = 0.08f;
    public float bobSpeed = 2f;
    public float heightOffset = 0.7f;
    public Color color = new Color(1f, 0.92f, 0.3f);

    Transform iconTransform;
    Vector3 basePos;

    void Awake()
    {
        var iconGO = new GameObject("HintIcon");
        iconGO.transform.SetParent(transform, false);
        iconGO.transform.localPosition = new Vector3(0f, heightOffset, 0f);

        var sr = iconGO.AddComponent<SpriteRenderer>();
        sr.sprite = MakeExclamationSprite();
        sr.sortingOrder = 20;
        sr.color = color;

        iconTransform = iconGO.transform;
        basePos = iconTransform.localPosition;
    }

    void Update()
    {
        if (iconTransform == null) return;
        float y = basePos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        iconTransform.localPosition = new Vector3(basePos.x, y, basePos.z);
    }

    static Sprite MakeExclamationSprite()
    {
        const int size = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        void FillRect(int x0, int y0, int x1, int y1)
        {
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                    pixels[y * size + x] = Color.white;
        }

        FillRect(6, 8, 10, 13); // bar (top of the "!")
        FillRect(6, 3, 10, 6);  // dot (bottom of the "!")

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}
