using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using Fusion;

public class GameUIManager : MonoBehaviour {
    [Header("UI Panels")]
    public GameObject panelLogin;
    public GameObject panelRegister; // Bảng Đăng Ký Mới
    public GameObject panelLobby;

    [Header("Login UI")]
    public InputField inputLoginUser;
    public InputField inputLoginPass;
    public Button btnLoginSubmit;
    public Button btnGoToRegister; // Nút chuyển sang Đăng ký

    [Header("Register UI")]
    public InputField inputRegUser;
    public InputField inputRegPass;
    public Button btnRegisterSubmit;
    public Button btnGoToLogin; // Nút quay về Đăng nhập

    [Header("Lobby UI")]
    public Text txtUsername;
    public Text txtLevel;
    public Text txtGold;
    public Text txtDiamonds;
    
    // --- PHẦN MỚI THÊM: DROP DOWNS CHỌN MAP & CHẾ ĐỘ ---
    [Header("Matchmaking Settings")]
    public Dropdown dropdownMap;  // Ô Chọn Map
    public Dropdown dropdownMode; // Ô Chọn Chế độ chơi
    // ---------------------------------------------------

    public Button btnFindMatch;
    public Button btnRefresh;
    public Button btnLogout;

    // private string apiUrl = "http://localhost:5015/api/main";
    // ĐỔI TỪ LOCALHOST SANG SERVER THẬT
    private string apiUrl = "http://gameserver.runasp.net/api/Main"; 
    private NetworkRunner _runner;

    void Start() {
        // Nút chuyển bảng (Login <-> Register)
        btnGoToRegister.onClick.AddListener(() => {
            panelLogin.SetActive(false);
            panelRegister.SetActive(true);
        });
        btnGoToLogin.onClick.AddListener(() => {
            panelRegister.SetActive(false);
            panelLogin.SetActive(true);
        });

        // Nút Gửi dữ liệu
        btnLoginSubmit.onClick.AddListener(OnLoginClicked);
        btnRegisterSubmit.onClick.AddListener(OnRegisterClicked);
        
        // Nút Sảnh
        btnFindMatch.onClick.AddListener(OnFindMatchClicked);
        btnRefresh.onClick.AddListener(OnRefreshClicked);
        btnLogout.onClick.AddListener(OnLogoutClicked);

        // Auto Login
        if (PlayerPrefs.HasKey("SavedUser") && PlayerPrefs.HasKey("SavedPass")) {
            panelLogin.SetActive(false);
            panelRegister.SetActive(false);
            StartCoroutine(LoginRoutine(PlayerPrefs.GetString("SavedUser"), PlayerPrefs.GetString("SavedPass")));
        } else {
            panelLogin.SetActive(true);
            panelRegister.SetActive(false);
            panelLobby.SetActive(false);
        }
    }

    // --- XỬ LÝ ĐĂNG KÝ ---
    void OnRegisterClicked() {
        string user = inputRegUser.text;
        string pass = inputRegPass.text;
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) {
            Debug.LogWarning("Vui lòng nhập đủ thông tin Đăng ký!"); return;
        }
        btnRegisterSubmit.interactable = false;
        StartCoroutine(RegisterRoutine(user, pass));
    }

    IEnumerator RegisterRoutine(string username, string password) {
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/register/{username}/{password}")) {
            yield return www.SendWebRequest();
            btnRegisterSubmit.interactable = true;

            if (www.result == UnityWebRequest.Result.Success) {
                Debug.Log("Đăng ký thành công! Hãy Đăng nhập.");
                // Chuyển về màn hình Login và điền sẵn tên
                panelRegister.SetActive(false);
                panelLogin.SetActive(true);
                inputLoginUser.text = username;
            } else {
                // In ra lỗi từ Server (ví dụ: "Tên tài khoản đã tồn tại!")
                Debug.LogError("Lỗi Đăng ký: " + www.downloadHandler.text);
            }
        }
    }

    // --- XỬ LÝ ĐĂNG NHẬP ---
    void OnLoginClicked() {
        string user = inputLoginUser.text;
        string pass = inputLoginPass.text;
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) {
            Debug.LogWarning("Vui lòng nhập đủ thông tin Đăng nhập!"); return;
        }
        btnLoginSubmit.interactable = false; 
        StartCoroutine(LoginRoutine(user, pass));
    }

    IEnumerator LoginRoutine(string username, string password) {
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/login/{username}/{password}")) {
            yield return www.SendWebRequest();
            btnLoginSubmit.interactable = true;

            if (www.result == UnityWebRequest.Result.Success) {
                PlayerData data = JsonUtility.FromJson<PlayerData>(www.downloadHandler.text);
                Debug.Log("Đăng nhập thành công!");
                PlayerPrefs.SetString("SavedUser", username);
                PlayerPrefs.SetString("SavedPass", password);
                PlayerPrefs.Save();
                UpdateLobbyUI(data);
            } else {
                Debug.LogError("Lỗi Đăng nhập: " + www.downloadHandler.text);
                panelLogin.SetActive(true);
                panelLobby.SetActive(false);
            }
        }
    }

    void OnLogoutClicked() {
        PlayerPrefs.DeleteKey("SavedUser");
        PlayerPrefs.DeleteKey("SavedPass");
        PlayerPrefs.Save();
        panelLobby.SetActive(false);
        panelLogin.SetActive(true);
        inputLoginUser.text = "";
        inputLoginPass.text = "";
        Debug.Log("Đã đăng xuất!");
    }

    void UpdateLobbyUI(PlayerData data) {
        panelLogin.SetActive(false);
        panelRegister.SetActive(false);
        panelLobby.SetActive(true);

        txtUsername.text = "Tên: " + data.username;
        txtLevel.text = "Level: " + data.level.ToString();
        txtGold.text = "Vàng: " + data.gold.ToString();
        txtDiamonds.text = "Kim Cương: " + data.diamonds.ToString();
    }

    void OnRefreshClicked() {
        string user = PlayerPrefs.GetString("SavedUser");
        string pass = PlayerPrefs.GetString("SavedPass");
        StartCoroutine(RefreshRoutine(user, pass));
    }

    IEnumerator RefreshRoutine(string username, string password) {
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/login/{username}/{password}")) {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                PlayerData data = JsonUtility.FromJson<PlayerData>(www.downloadHandler.text);
                txtGold.text = "Vàng: " + data.gold.ToString();
                txtDiamonds.text = "Kim Cương: " + data.diamonds.ToString();
            }
        }
    }

    void OnFindMatchClicked() {
        btnFindMatch.interactable = false;
        StartFusion();
    }

    // --- ĐÃ NÂNG CẤP: LẤY THÔNG TIN TỪ DROPDOWN ĐỂ TÌM PHÒNG ---
    async void StartFusion() {
        GameObject runnerObj = new GameObject("Fusion_NetworkRunner");
        _runner = runnerObj.AddComponent<NetworkRunner>();
        var sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();

        // Lấy giá trị từ UI Dropdown
        int mapIndex = dropdownMap != null ? dropdownMap.value : 0; 
        string modeName = dropdownMode != null ? dropdownMode.options[dropdownMode.value].text : "Default";

        // Đặt tên phòng dựa theo Map và Chế độ
        string roomName = $"Phong_{modeName}_Map{mapIndex}";
        
        // Tính toán Scene cần load (Map 1 -> Scene 1, Map 2 -> Scene 2...)
        int sceneBuildIndexToLoad = mapIndex + 1;

        Debug.Log($"Đang tìm trận... Phòng: {roomName} | Đang load Scene số: {sceneBuildIndexToLoad}");

        var result = await _runner.StartGame(new StartGameArgs() {
            GameMode = GameMode.Shared,
            SessionName = roomName, // Ép vào chung phòng theo tùy chọn
            SceneManager = sceneManager, 
            Scene = SceneRef.FromIndex(sceneBuildIndexToLoad) // Load đúng màn chơi
        });
        
        if (result.Ok) {
            Debug.Log($"ĐÃ VÀO PHÒNG {roomName} THÀNH CÔNG!");
        } else {
            Debug.LogError("Lỗi vào phòng: " + result.ShutdownReason);
            btnFindMatch.interactable = true;
        }
    }
}

[System.Serializable]
public class PlayerData {
    public string id;
    public string username;
    public bool isBanned;
    public long gold;
    public long diamonds;
    public int level;
    public long exp;
    public string equippedCharacter;
}