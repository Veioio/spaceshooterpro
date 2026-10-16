using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-click wiring for the Game scene: creates the explosion and power-up prefabs,
// the HUD, the audio/game managers, and the player's shield and thruster visuals.
// Run it from the menu: Space Shooter > Setup Game Scene. It is safe to run repeatedly.
public static class SpaceShooterSetup
{
    private const string ScenePath = "Assets/Scenes/Game.unity";
    private const string PrefabFolder = "Assets/Prefabs";

    [MenuItem("Space Shooter/Setup Game Scene")]
    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Player player = Object.FindObjectOfType<Player>();
        SpawnManager spawnManager = Object.FindObjectOfType<SpawnManager>();

        if (player == null || spawnManager == null)
        {
            Debug.LogError("Setup needs a Player and a Spawn_Manager in the Game scene.");
            return;
        }

        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();

        GameObject explosionPrefab = CreateExplosionPrefab(playerRenderer);
        GameObject[] powerUpPrefabs = new GameObject[]
        {
            CreatePowerUpPrefab("PowerUp_TripleShot", PowerUpType.TripleShot, "Assets/Sprites/Power_Ups/Triple_Shot", playerRenderer),
            CreatePowerUpPrefab("PowerUp_Speed", PowerUpType.Speed, "Assets/Sprites/Power_Ups/Speed", playerRenderer),
            CreatePowerUpPrefab("PowerUp_Shield", PowerUpType.Shield, "Assets/Sprites/Power_Ups/Shield", playerRenderer)
        };

        SetupPlayerVisuals(player, playerRenderer);
        UIManager uiManager = SetupCanvas();
        SetupGameManager(uiManager, explosionPrefab);

        SerializedObject spawnerObject = new SerializedObject(spawnManager);
        SetObjectArray(spawnerObject.FindProperty("_powerUpPrefabs"), powerUpPrefabs);
        spawnerObject.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        EditorSceneManager.SaveScene(player.gameObject.scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Space Shooter setup complete. Press Play to test.");
    }

    // ---------------------------------------------------------------- prefabs

    private static GameObject CreateExplosionPrefab(SpriteRenderer reference)
    {
        // The source sequence has ~158 frames; every 3rd keeps the animation short and light.
        Sprite[] frames = LoadSprites("Assets/Sprites/Explosion", 3);

        GameObject go = new GameObject("Explosion");
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = frames.Length > 0 ? frames[0] : null;
        ApplySorting(spriteRenderer, reference, 5);

        SpriteAnimator animator = go.AddComponent<SpriteAnimator>();
        SerializedObject animatorObject = new SerializedObject(animator);
        SetObjectArray(animatorObject.FindProperty("_frames"), frames);
        animatorObject.FindProperty("_framesPerSecond").floatValue = 40f;
        animatorObject.FindProperty("_loop").boolValue = false;
        animatorObject.FindProperty("_destroyOnEnd").boolValue = true;
        animatorObject.ApplyModifiedProperties();

        return SavePrefab(go, "Explosion");
    }

    private static GameObject CreatePowerUpPrefab(string name, PowerUpType type, string spriteFolder, SpriteRenderer reference)
    {
        Sprite[] frames = LoadSprites(spriteFolder, 1);

        GameObject go = new GameObject(name);
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = frames.Length > 0 ? frames[0] : null;
        ApplySorting(spriteRenderer, reference, 1);

        // Scale the sprite so it is roughly 1.2 world units across.
        if (spriteRenderer.sprite != null)
        {
            Vector3 size = spriteRenderer.sprite.bounds.size;
            float scale = 1.2f / Mathf.Max(size.x, size.y);
            go.transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Sprites carry transparent padding, so the collider is a bit smaller than the sprite.
        BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = collider.size * 0.5f;

        Rigidbody2D body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;

        SpriteAnimator animator = go.AddComponent<SpriteAnimator>();
        SerializedObject animatorObject = new SerializedObject(animator);
        SetObjectArray(animatorObject.FindProperty("_frames"), frames);
        animatorObject.FindProperty("_framesPerSecond").floatValue = 20f;
        animatorObject.ApplyModifiedProperties();

        PowerUp powerUp = go.AddComponent<PowerUp>();
        SerializedObject powerUpObject = new SerializedObject(powerUp);
        powerUpObject.FindProperty("_type").enumValueIndex = (int)type;
        powerUpObject.ApplyModifiedProperties();

        return SavePrefab(go, name);
    }

    private static GameObject SavePrefab(GameObject go, string name)
    {
        string path = PrefabFolder + "/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ----------------------------------------------------------------- player

    private static void SetupPlayerVisuals(Player player, SpriteRenderer playerRenderer)
    {
        RemoveChild(player.transform, "Shield");
        RemoveChild(player.transform, "Thruster");

        Vector3 playerSize = playerRenderer.sprite != null ? playerRenderer.sprite.bounds.size : Vector3.one;

        // Shield: a looping animation around the ship, switched on by the shield power-up.
        Sprite[] shieldFrames = LoadSprites("Assets/Sprites/Player_Shield", 1);
        GameObject shield = CreateAnimatedChild(player.transform, "Shield", shieldFrames, 24f, playerRenderer, 1);
        if (shieldFrames.Length > 0)
        {
            Vector3 shieldSize = shieldFrames[0].bounds.size;
            float scale = 1.3f * Mathf.Max(playerSize.x, playerSize.y) / Mathf.Max(shieldSize.x, shieldSize.y);
            shield.transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Thruster: a looping flame under the ship. Adjust its position in the scene if it looks off.
        Sprite[] thrusterFrames = LoadSprites("Assets/Sprites/Thruster", 1);
        GameObject thruster = CreateAnimatedChild(player.transform, "Thruster", thrusterFrames, 30f, playerRenderer, -1);
        if (thrusterFrames.Length > 0)
        {
            Vector3 thrusterSize = thrusterFrames[0].bounds.size;
            float scale = 0.6f * playerSize.x / Mathf.Max(thrusterSize.x, thrusterSize.y);
            thruster.transform.localScale = new Vector3(scale, scale, 1f);
            thruster.transform.localPosition = new Vector3(0f, -0.5f * playerSize.y, 0f);
        }

        SerializedObject playerObject = new SerializedObject(player);
        playerObject.FindProperty("_shieldVisual").objectReferenceValue = shield;
        playerObject.ApplyModifiedProperties();
    }

    private static GameObject CreateAnimatedChild(Transform parent, string name, Sprite[] frames, float fps, SpriteRenderer reference, int orderOffset)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);

        SpriteRenderer spriteRenderer = child.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = frames.Length > 0 ? frames[0] : null;
        ApplySorting(spriteRenderer, reference, orderOffset);

        SpriteAnimator animator = child.AddComponent<SpriteAnimator>();
        SerializedObject animatorObject = new SerializedObject(animator);
        SetObjectArray(animatorObject.FindProperty("_frames"), frames);
        animatorObject.FindProperty("_framesPerSecond").floatValue = fps;
        animatorObject.ApplyModifiedProperties();

        return child;
    }

    // --------------------------------------------------------------------- UI

    private static UIManager SetupCanvas()
    {
        GameObject old = GameObject.Find("Canvas");
        if (old != null)
        {
            Object.DestroyImmediate(old);
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Transform canvas = canvasObject.transform;
        Text score = CreateText(canvas, "Score_Text", "Score: 0", 56, TextAnchor.UpperLeft, Color.white,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), new Vector2(700, 90));
        Text gameOver = CreateText(canvas, "GameOver_Text", "GAME OVER", 130, TextAnchor.MiddleCenter, Color.red,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1500, 220));
        Text restart = CreateText(canvas, "Restart_Text", "Press R to restart", 52, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(1200, 90));

        Sprite[] livesSprites = new Sprite[]
        {
            LoadSprite("Assets/Sprites/UI/Lives/no_lives.png"),
            LoadSprite("Assets/Sprites/UI/Lives/One.png"),
            LoadSprite("Assets/Sprites/UI/Lives/Two.png"),
            LoadSprite("Assets/Sprites/UI/Lives/Three.png")
        };

        GameObject livesObject = new GameObject("Lives_Image", typeof(RectTransform), typeof(Image));
        livesObject.transform.SetParent(canvas, false);
        RectTransform livesRect = livesObject.GetComponent<RectTransform>();
        livesRect.anchorMin = livesRect.anchorMax = livesRect.pivot = new Vector2(1, 1);
        livesRect.anchoredPosition = new Vector2(-30, -30);
        livesRect.sizeDelta = new Vector2(256, 128);
        Image livesImage = livesObject.GetComponent<Image>();
        livesImage.sprite = livesSprites[3];
        livesImage.preserveAspect = true;

        UIManager uiManager = canvasObject.AddComponent<UIManager>();
        SerializedObject uiObject = new SerializedObject(uiManager);
        uiObject.FindProperty("_scoreText").objectReferenceValue = score;
        uiObject.FindProperty("_livesImage").objectReferenceValue = livesImage;
        SetObjectArray(uiObject.FindProperty("_livesSprites"), livesSprites);
        uiObject.FindProperty("_gameOverText").objectReferenceValue = gameOver;
        uiObject.FindProperty("_restartText").objectReferenceValue = restart;
        uiObject.ApplyModifiedProperties();

        return uiManager;
    }

    private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    // ------------------------------------------------------------ game manager

    private static void SetupGameManager(UIManager uiManager, GameObject explosionPrefab)
    {
        GameObject old = GameObject.Find("Game_Manager");
        if (old != null)
        {
            Object.DestroyImmediate(old);
        }

        GameObject managerObject = new GameObject("Game_Manager");

        GameManager gameManager = managerObject.AddComponent<GameManager>();
        SerializedObject gameManagerObject = new SerializedObject(gameManager);
        gameManagerObject.FindProperty("_uiManager").objectReferenceValue = uiManager;
        gameManagerObject.ApplyModifiedProperties();

        GameAssets assets = managerObject.AddComponent<GameAssets>();
        SerializedObject assetsObject = new SerializedObject(assets);
        assetsObject.FindProperty("_laserClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/laser_shot.wav");
        assetsObject.FindProperty("_explosionClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/explosion_sound.wav");
        assetsObject.FindProperty("_powerUpClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/power_up_sound.wav");
        assetsObject.FindProperty("_explosionPrefab").objectReferenceValue = explosionPrefab;
        assetsObject.ApplyModifiedProperties();

        AudioSource music = managerObject.AddComponent<AudioSource>();
        music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/music_background.wav");
        music.loop = true;
        music.playOnAwake = true;
        music.volume = 0.4f;
    }

    // ---------------------------------------------------------------- helpers

    // Loads the sprites in a folder in file-name order, keeping every `step`-th one.
    private static Sprite[] LoadSprites(string folder, int step)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new string[] { folder });
        List<string> paths = new List<string>();
        foreach (string guid in guids)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }
        paths.Sort(System.StringComparer.Ordinal);

        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < paths.Count; i += step)
        {
            Sprite sprite = LoadSprite(paths[i]);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }
        return sprites.ToArray();
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning("Sprite not found: " + path);
        }
        return sprite;
    }

    private static void SetObjectArray(SerializedProperty property, Object[] items)
    {
        property.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }

    private static void ApplySorting(SpriteRenderer target, SpriteRenderer reference, int orderOffset)
    {
        if (reference == null)
        {
            return;
        }
        target.sortingLayerID = reference.sortingLayerID;
        target.sortingOrder = reference.sortingOrder + orderOffset;
    }

    private static void RemoveChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }
}
