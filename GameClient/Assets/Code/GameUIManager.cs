using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using Fusion;
using System.Collections.Generic;

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

        // FindObjectOfType<SocialManager>().StartSocialFeatures();
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
    // --- NÂNG CẤP: MATCHMAKING TỰ ĐỘNG THEO CHẾ ĐỘ SOLO / TEAM 2 / TEAM 4 ---
    async void StartFusion() {
        // 1. Tạo Network Runner
        GameObject runnerObj = new GameObject("Fusion_NetworkRunner");
        _runner = runnerObj.AddComponent<NetworkRunner>();
        var sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();

        // 2. Lấy lựa chọn từ Dropdown
        int mapIndex = dropdownMap != null ? dropdownMap.value : 0; 
        string modeName = dropdownMode != null ? dropdownMode.options[dropdownMode.value].text : "Solo";

        // 3. Quy định số người tối đa cho từng chế độ
        int maxPlayers = 8; // Mặc định
        if (modeName.Contains("Solo")) maxPlayers = 8;       // 8 người bắn tự do
        else if (modeName.Contains("2")) maxPlayers = 4;     // 2 vs 2 (Tổng 4 người)
        else if (modeName.Contains("4")) maxPlayers = 8;     // 4 vs 4 (Tổng 8 người)

        // 4. Gắn thẻ (Tag) cho phòng để ghép đúng đối tượng
        var customProps = new Dictionary<string, SessionProperty> {
            { "GameMode", modeName }, // Lọc đúng chế độ (Solo/Team2/Team4)
            { "MapIndex", mapIndex }  // Lọc đúng Map
        };

        int sceneBuildIndexToLoad = mapIndex + 1; // Giả sử Map 0 tương ứng Scene 1
        Debug.Log($"Đang tìm trận... Chế độ: {modeName} | Map: {mapIndex} | Max: {maxPlayers} người");

        // 5. Bắt đầu ghép trận
        var result = await _runner.StartGame(new StartGameArgs() {
            GameMode = GameMode.Shared, 
            
            // QUAN TRỌNG NHẤT: Bỏ trống SessionName để Fusion TỰ ĐỘNG TÌM PHÒNG.
            // Nếu không có phòng nào khớp, nó sẽ TỰ TẠO PHÒNG MỚI.
            SessionName = "", 
            
            SessionProperties = customProps, // Bắt buộc trùng khớp Mode và Map mới cho vào chung
            PlayerCount = maxPlayers,        // Giới hạn số người trong phòng
            SceneManager = sceneManager, 
            Scene = SceneRef.FromIndex(sceneBuildIndexToLoad)
        });
        
        if (result.Ok) {
            Debug.Log($"ĐÃ VÀO PHÒNG CHẾ ĐỘ [{modeName}] THÀNH CÔNG!");
            // Vì là Shared Mode, người chơi sẽ vào phòng và chơi được luôn dù chưa đủ người.
        } else {
            Debug.LogError("Lỗi vào phòng: " + result.ShutdownReason);
            btnFindMatch.interactable = true; // Bật lại nút nếu lỗi
        }
    }


    // Hàm này được SocialManager gọi khi bấm "Chấp nhận" lời mời
public async void JoinRoomFromInvite(string roomName, string modeName) {
    GameObject runnerObj = new GameObject("Fusion_NetworkRunner");
    _runner = runnerObj.AddComponent<NetworkRunner>();
    var sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();

    int maxPlayers = modeName.Contains("2") ? 4 : 8;

    // Phải thiết lập đúng GameMode để Fusion cho vào phòng
    var customProps = new System.Collections.Generic.Dictionary<string, SessionProperty> {
        { "GameMode", modeName } 
    };

    Debug.Log($"Vào phòng bạn mời: {roomName}");

    var result = await _runner.StartGame(new StartGameArgs() {
        GameMode = GameMode.Shared, 
        SessionName = roomName, // Ép vào đúng cái tên phòng bạn mình gửi
        SessionProperties = customProps,
        PlayerCount = maxPlayers,
        SceneManager = sceneManager, 
        Scene = SceneRef.FromIndex(1) // Tuỳ chỉnh index Map sau
    });
    
    if (result.Ok) {
        Debug.Log("ĐÃ VÀO PHÒNG BẠN BÈ THÀNH CÔNG!");
    } else {
        Debug.LogError("Lỗi vào phòng: " + result.ShutdownReason);
    }
}

// CHỦ PHÒNG GỌI HÀM NÀY ĐỂ VÀO ĐỨNG CHỜ BẠN BÈ
    public async void HostRoomForFriend(string roomName, string modeName) {
        GameObject runnerObj = new GameObject("Fusion_NetworkRunner");
        _runner = runnerObj.AddComponent<NetworkRunner>();
        var sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();

        // Giới hạn người chơi: Team 2 (4 người), Team 4 (8 người)
        int maxPlayers = modeName.Contains("2") ? 4 : 8;

        var customProps = new System.Collections.Generic.Dictionary<string, SessionProperty> {
            { "GameMode", modeName } 
        };

        Debug.Log($"[HOST] Đang tạo phòng bí mật: {roomName} chế độ {modeName}");

        var result = await _runner.StartGame(new StartGameArgs() {
            GameMode = GameMode.Shared, 
            SessionName = roomName, // ÉP ĐÚNG TÊN PHÒNG NÀY (Không được để trống)
            SessionProperties = customProps,
            PlayerCount = maxPlayers,
            SceneManager = sceneManager, 
            Scene = SceneRef.FromIndex(1) // Thay đổi số 1 thành Index Map của Tiến
        });
        
        if (result.Ok) {
            Debug.Log($"[HOST] ĐÃ TẠO PHÒNG XONG. HÃY ĐỨNG CHỜ BẠN BÈ BẤM CHẤP NHẬN!");
            // (Tùy chọn) Ẩn Lobby UI đi, hiện màn hình Loading Game
            panelLobby.SetActive(false); 
        } else {
            Debug.LogError("Lỗi tạo phòng: " + result.ShutdownReason);
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