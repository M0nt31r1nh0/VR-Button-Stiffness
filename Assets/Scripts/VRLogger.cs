using System.IO;
using UnityEngine;

public class VRLogger : MonoBehaviour
{
    public static VRLogger Instance;

    [Header("Camera to log")]
    public Transform cameraTransform;          // assign XR camera here
    public string sessionId = "02052024_161606";

    [Header("Logging settings")]
    public float logInterval = 1.0f;           // log every X seconds
    public string fileName = "vr_log.csv";

    private string filePath;
    private float timer;
    private int frameCounter;

    void Awake()
    {
        // Simple singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Write header if file is new
        if (!File.Exists(filePath))
        {
            string header = "frame,timecode,posX,posY,posZ,rotX,rotY,rotZ,eventName,sessionId,elapsedTime\n";
            File.WriteAllText(filePath, header);
        }

        timer = 0f;
        frameCounter = 0;
    }

    void Update()
    {
        if (cameraTransform == null)
            return;

        timer += Time.deltaTime;
        frameCounter++;

        // Example: log camera every X seconds
        if (timer >= logInterval)
        {
            LogTransform("cameraSample", cameraTransform, timer);
            timer = 0f;
        }
    }

    // Generic transform logger (camera or grabbed object)
    private void LogTransform(string eventName, Transform t, float elapsedTime)
    {
        Vector3 pos = t.position;
        Quaternion rot = t.rotation;

        string timecode = FormatTimecode(Time.time);

        string line = string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\n",
            frameCounter,
            timecode,
            pos.x.ToString("F2"),
            pos.y.ToString("F2"),
            pos.z.ToString("F2"),
            rot.x.ToString("F7"),
            rot.y.ToString("F7"),
            rot.z.ToString("F7"),
            eventName,
            sessionId,
            elapsedTime.ToString("F3")
        );

        File.AppendAllText(filePath, line);
    }

    /// Call this from other scripts (e.g. grab events)
    public void LogEvent(string eventName, Transform referenceTransform = null)
    {
        if (cameraTransform == null && referenceTransform == null)
            return;

        frameCounter++;

        Transform t = referenceTransform != null ? referenceTransform : cameraTransform;

        Vector3 pos = t.position;
        Quaternion rot = t.rotation;

        string timecode = FormatTimecode(Time.time);
        float elapsed = Time.timeSinceLevelLoad;

        string line = string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\n",
            frameCounter,
            timecode,
            pos.x.ToString("F2"),
            pos.y.ToString("F2"),
            pos.z.ToString("F2"),
            rot.x.ToString("F7"),
            rot.y.ToString("F7"),
            rot.z.ToString("F7"),
            eventName,
            sessionId,
            elapsed.ToString("F3")
        );

        File.AppendAllText(filePath, line);
    }

    // 00:00:00:016 style timecode
    private string FormatTimecode(float timeSeconds)
    {
        int totalMilliseconds = Mathf.FloorToInt(timeSeconds * 1000f);
        int hours = totalMilliseconds / (1000 * 60 * 60);
        int minutes = (totalMilliseconds / (1000 * 60)) % 60;
        int seconds = (totalMilliseconds / 1000) % 60;
        int millis = totalMilliseconds % 1000;

        return string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, millis);
    }
}
