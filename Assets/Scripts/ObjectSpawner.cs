using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("🎯 Префабы объектов")]
    public GameObject planetPrefab;
    public GameObject enemyPrefab;
    public GameObject blackHolePrefab;

    [Header("📏 Масштабы объектов")]
    public Vector3 planetScale = Vector3.one;
    public Vector3 enemyScale = Vector3.one;
    public Vector3 blackHoleScale = Vector3.one;

    [Header("📦 Размеры пулов")]
    public int poolSizePlanets = 10;
    public int poolSizeEnemies = 10;
    public int poolSizeBlackHoles = 2;

    [Header("🌐 Параметры спавна")]
    public float spawnRadius = 10f;
    public float respawnDelay = 2f;

    [Header("🎯 Центр спавна")]
    public Transform spawnCenter;

    [Header("🎨 Источник цвета (UI Image)")]
    [SerializeField] private Image colorSourceImage;

    private List<GameObject> planetPool = new();
    private List<GameObject> enemyPool = new();
    private List<GameObject> blackHolePool = new();
    private HashSet<GameObject> pendingRespawns = new();

    private Color lastPlanetColor = Color.white;

    private void Start()
    {
        if (colorSourceImage != null)
        {
            lastPlanetColor = colorSourceImage.color;
        }

        InitPool("Planet", planetPrefab, poolSizePlanets, planetScale, planetPool);
        InitPool("Enemy", enemyPrefab, poolSizeEnemies, enemyScale, enemyPool);
        InitPool("TheBlackHole", blackHolePrefab, poolSizeBlackHoles, blackHoleScale, blackHolePool);
    }

    private void Update()
    {
        if (colorSourceImage == null) return;

        Color current = colorSourceImage.color;
        if (!ColorsAreSimilar(current, lastPlanetColor))
        {
            lastPlanetColor = current;

            foreach (var obj in planetPool)
            {
                if (obj.TryGetComponent<SpriteRenderer>(out var sr))
                    sr.color = current;
            }
        }
    }

    private void InitPool(string tag, GameObject prefab, int count, Vector3 scale, List<GameObject> pool)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.tag = tag;
            obj.transform.localScale = scale;
            obj.SetActive(false);

            if (obj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                if (tag == "Planet") sr.color = colorSourceImage != null ? colorSourceImage.color : Color.white;
                else if (tag == "Enemy") sr.color = GenerateDistinctColor(colorSourceImage != null ? colorSourceImage.color : Color.white);
                else if (tag == "TheBlackHole") sr.color = Color.black;
            }

            pool.Add(obj);
            RequestRespawn(tag, initialDelay: true);
        }
    }

    public void RequestRespawn(string tag, bool initialDelay = false)
    {
        var pool = GetPoolByTag(tag);
        if (pool == null) return;

        GameObject obj = pool.Find(o => !o.activeInHierarchy && !pendingRespawns.Contains(o));
        if (obj != null)
        {
            pendingRespawns.Add(obj);
            float delay = initialDelay ? Random.Range(0f, 0.5f) : respawnDelay;
            StartCoroutine(RespawnObject(obj, tag, delay));
        }
    }

    private IEnumerator RespawnObject(GameObject obj, string tag, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj == null || obj.activeInHierarchy)
        {
            pendingRespawns.Remove(obj);
            yield break;
        }

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 center = spawnCenter ? spawnCenter.position : transform.position;
        Vector3 spawnPos = center + new Vector3(offset.x, offset.y, 0f);

        obj.transform.position = spawnPos;
        obj.SetActive(true);

        pendingRespawns.Remove(obj);
    }

    private List<GameObject> GetPoolByTag(string tag) => tag switch
    {
        "Planet" => planetPool,
        "Enemy" => enemyPool,
        "TheBlackHole" => blackHolePool,
        _ => null
    };

    private Color GenerateDistinctColor(Color avoid)
    {
        Color random;
        int tries = 0;
        do
        {
            random = new Color(Random.value, Random.value, Random.value);
            tries++;
        } while (ColorsAreSimilar(random, avoid) && tries < 10);
        return random;
    }

    private bool ColorsAreSimilar(Color a, Color b, float threshold = 0.15f)
    {
        float diff = Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        return diff < threshold;
    }
}
