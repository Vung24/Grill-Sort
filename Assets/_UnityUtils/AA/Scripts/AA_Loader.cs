using UnityEngine;
using UnityEngine.UI;
#if ADDRESSABLE_AVAILABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using System.Collections;


public class AA_Loader : MonoBehaviour
{
#if ADDRESSABLE_AVAILABLE
    public static AA_Loader Instance;

    [SerializeField] private Button[] button;
    [SerializeField] private GameObject debugUI, downloading;
    [SerializeField] private Button buttonClose;
    private int _index;
    private int[] _correctIndex = new[]
    {
        0, 2, 1
    };
    
    [Header("DEBUG_UI")]
    public Text txtStatus;
    public Text txtPercent;
    public Text txtSpeed;
    public Text txtSize;

    private float lastTime;
    private long lastBytes;
    
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
    public Color doneColor = Color.green;

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < button.Length; i++)
        {
            var temp = i;
            button[i].onClick.AddListener(() => CheckDebug(temp));
        }
        
        buttonClose.onClick.AddListener(() => debugUI.gameObject.SetActive(false));
    }

    private void CheckDebug(int i)
    {
        if (_correctIndex[_index] == i)
        {
            _index++;
            if (_index >= 3)
            {
                _index = 0;
                debugUI.gameObject.SetActive(!debugUI.activeSelf);
            }
        }
        else
        {
            _index = 0;
        }
        
           
    }
    

    public void LoadMiniGame(string label)
    {
        downloading.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(LoadRoutine(label));
    }

    private IEnumerator LoadRoutine(string label)
    {
        SetStatus("Checking size...");
        ResetUI();

        var sizeHandle = Addressables.GetDownloadSizeAsync(label);
        yield return sizeHandle;

        long totalSize = sizeHandle.Result;
        
        txtSize.text = $"0 MB/{totalSize / 1024f / 1024f:F2} MB";

        if (totalSize > 0)
        {
            SetStatus("Downloading...");
            lastTime = Time.realtimeSinceStartup;
            lastBytes = 0;

            var downloadHandle = Addressables.DownloadDependenciesAsync(label);

            while (!downloadHandle.IsDone)
            {
                float percent = downloadHandle.PercentComplete * 100f;
                txtPercent.text = $"{percent:F1}%";
                
                txtSize.text = $"{ (downloadHandle.PercentComplete * totalSize) / 1024f / 1024f:F2} MB/{totalSize / 1024f / 1024f:F2} MB";

                UpdateSpeed(downloadHandle.GetDownloadStatus().DownloadedBytes);
                yield return null;
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                var ex = downloadHandle.OperationException;
                if (ex != null)
                {
                    txtStatus.text = "Error: " + ex.Message;
                }
                else
                {
                    txtStatus.text = "Error: Unknown";
                }
                yield break;
            }

        }
        else
        {
            SetStatus("Already cached");
        }

        SetStatus("Loading scene...");
        yield return Addressables.LoadSceneAsync(label);

        SetStatus("Done");
        downloading.SetActive(false);
    }

    private void UpdateSpeed(long downloadedBytes)
    {
        float now = Time.realtimeSinceStartup;
        float deltaTime = now - lastTime;

        if (deltaTime < 0.5f) return;

        long deltaBytes = downloadedBytes - lastBytes;
        float speed = deltaBytes / deltaTime;

        txtSpeed.text = speed > 1024 * 1024
            ? $"{speed / 1024f / 1024f:F2} MB/s"
            : $"{speed / 1024f:F1} KB/s";

        lastTime = now;
        lastBytes = downloadedBytes;
    }

    private void ResetUI()
    {
        txtPercent.text = "0%";
        txtSpeed.text = "0 KB/s";
        txtSize.text = "--";
    }

    private void SetStatus(string status)
    {
        txtStatus.text = $"{status}";
    }
#endif
}
