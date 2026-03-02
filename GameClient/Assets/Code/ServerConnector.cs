using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Fusion; 


public class ServerConnector : MonoBehaviour {
    // Thay số 5015 bằng số Port bạn thấy ở Terminal
    private string apiUrl = "http://gameserver.runasp.net/api/Main";    
    private NetworkRunner _runner;

    void Start() {
        // SỬA DÒNG NÀY: Truyền thêm mật khẩu vào (ví dụ "123")
        StartCoroutine(LoginRoutine("Tien123", "123")); 
    }

    // SỬA DÒNG NÀY: Khai báo thêm biến password
    IEnumerator LoginRoutine(string username, string password) {
        
        // SỬA DÒNG NÀY: Nối thêm password vào URL để khớp với Backend mới
        // using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/login/{username}/{password}")) {
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/login/{username}/{password}")) {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) {
                Debug.Log("Dữ liệu từ Server: " + www.downloadHandler.text);
                
                if (www.downloadHandler.text.Contains("\"isBanned\":true")) {
                    Debug.LogError("Tài khoản đã bị Ban! Không cho vào game.");
                } else {
                    Debug.Log("Tài khoản hợp lệ. Bắt đầu bật Fusion 2...");
                    StartFusion();
                }
            } else {
                Debug.LogError("Lỗi kết nối Server: " + www.error); 
            }
        }
    }

    async void StartFusion() {
        // KHẮC PHỤC LỖI "REUSED RUNNER": Luôn tạo một Game Object mới tinh
        GameObject runnerObj = new GameObject("Fusion_NetworkRunner");
        _runner = runnerObj.AddComponent<NetworkRunner>();
        var sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();

        Debug.Log("Đang kết nối vào phòng...");

        // Chạy Fusion
        var result = await _runner.StartGame(new StartGameArgs() {
            GameMode = GameMode.Shared,
            SessionName = "Lobby_Chung", 
            SceneManager = sceneManager, // Truyền biến an toàn vào đây
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
        });
        
        if (result.Ok) {
            Debug.Log("Đã vào phòng chơi thành công!");
        } else {
            Debug.LogError("Lỗi vào phòng: " + result.ShutdownReason);
        }
    }
}