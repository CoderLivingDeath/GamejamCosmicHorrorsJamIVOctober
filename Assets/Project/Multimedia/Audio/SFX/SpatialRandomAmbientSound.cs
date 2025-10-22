using UnityEngine;

public class SpatialRandomAmbientSound : MonoBehaviour
{
    public AudioClip[] ambientClips; // ������ ���������� ������
    public int audioSourceCount = 3; // ���������� ���������������
    public float minDelay = 5f;      // ����������� �������� ����� �������������
    public float maxDelay = 20f;     // ������������ �������� ����� �������������
    public float radius = 10f;       // ������ ������ ���������, ��� ���������� �����

    private AudioSource[] audioSources;
    private float[] nextPlayTime;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = this.transform; // �����������, ��� ������ �� ���������

        // ������� ������ ���������������
        audioSources = new AudioSource[audioSourceCount];
        nextPlayTime = new float[audioSourceCount];

        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject go = new GameObject("AmbientAudioSource_" + i);
            go.transform.parent = playerTransform;
            audioSources[i] = go.AddComponent<AudioSource>();
            audioSources[i].spatialBlend = 1.0f; // 3D ����
            nextPlayTime[i] = Time.time + Random.Range(minDelay, maxDelay);
        }
    }

    void Update()
    {
        for (int i = 0; i < audioSourceCount; i++)
        {
            if (Time.time >= nextPlayTime[i] && !audioSources[i].isPlaying && ambientClips.Length > 0)
            {
                // ��������� ������� ������ ������ � �������� radius
                Vector3 randomPos = playerTransform.position + Random.insideUnitSphere * radius;
                randomPos.y = playerTransform.position.y; // ���������� �� ������ ������

                audioSources[i].transform.position = randomPos;
                audioSources[i].clip = ambientClips[Random.Range(0, ambientClips.Length)];
                audioSources[i].Play();

                nextPlayTime[i] = Time.time + Random.Range(minDelay, maxDelay);
            }
        }
    }
}
