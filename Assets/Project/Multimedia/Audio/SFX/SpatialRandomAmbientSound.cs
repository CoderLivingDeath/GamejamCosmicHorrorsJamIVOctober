using UnityEngine;

public class SpatialRandomAmbientSound : MonoBehaviour
{
    public AudioClip[] ambientClips; // массив эмбиентных звуков
    public int audioSourceCount = 3; // количество аудиоисточников
    public float minDelay = 5f;      // минимальный интервал между проигрыванием
    public float maxDelay = 20f;     // максимальный интервал между проигрыванием
    public float radius = 10f;       // радиус вокруг персонажа, где появляются звуки

    private AudioSource[] audioSources;
    private float[] nextPlayTime;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = this.transform; // предположим, что скрипт на персонаже

        // создаем массив аудиоисточников
        audioSources = new AudioSource[audioSourceCount];
        nextPlayTime = new float[audioSourceCount];

        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject go = new GameObject("AmbientAudioSource_" + i);
            go.transform.parent = playerTransform;
            audioSources[i] = go.AddComponent<AudioSource>();
            audioSources[i].spatialBlend = 1.0f; // 3D звук
            nextPlayTime[i] = Time.time + Random.Range(minDelay, maxDelay);
        }
    }

    void Update()
    {
        for (int i = 0; i < audioSourceCount; i++)
        {
            if (Time.time >= nextPlayTime[i] && !audioSources[i].isPlaying && ambientClips.Length > 0)
            {
                // Случайная позиция вокруг игрока в пределах radius
                Vector3 randomPos = playerTransform.position + Random.insideUnitSphere * radius;
                randomPos.y = playerTransform.position.y; // закрепляем по высоте игрока

                audioSources[i].transform.position = randomPos;
                audioSources[i].clip = ambientClips[Random.Range(0, ambientClips.Length)];
                audioSources[i].Play();

                nextPlayTime[i] = Time.time + Random.Range(minDelay, maxDelay);
            }
        }
    }
}
