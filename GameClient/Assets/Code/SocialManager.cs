// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Networking;
// using System.Collections;

// public class SocialManager : MonoBehaviour {
//     // API Của Server
//     private string apiUrl = "http://gameserver.runasp.net/api/Social";
    
//     [Header("Friend UI")]
//     public InputField inputFriendName;
//     public Button btnAddFriend;

//     [Header("Invite Popup UI")]
//     public GameObject panelInvitePopup;
//     public Text txtInviteMessage;
//     public Button btnAcceptInvite;
//     public Button btnDeclineInvite;

//     [Header("Gửi Lời Mời UI")]
//     public InputField inputInviteName; // Ô nhập tên người muốn mời
//     public Dropdown dropdownInviteMode; // Chọn chế độ (Team 2, Team 4)
//     public Button btnSendInvite; // Nút bấm Mời

//     // Biến lưu thông tin lời mời đang chờ
//     private string pendingRoomName = "";
//     private string pendingGameMode = "";
//     private bool isCheckingInvites = false;

//     void Start() {
//         panelInvitePopup.SetActive(false);
        
//         btnAddFriend.onClick.AddListener(SendFriendRequest);
//         btnAcceptInvite.onClick.AddListener(AcceptInvite);
//         btnDeclineInvite.onClick.AddListener(() => panelInvitePopup.SetActive(false));
//         if (btnSendInvite != null) {
//             btnSendInvite.onClick.AddListener(OnInviteAndHostClicked);
//         }
//     }

//     void OnInviteAndHostClicked() {
//         string targetUser = inputInviteName.text;
//         string myUser = PlayerPrefs.GetString("SavedUser", "");
//         if (string.IsNullOrEmpty(targetUser)) {
//             Debug.LogWarning("Chưa nhập tên người muốn mời!");
//             return;
//         }

//         // Lấy chế độ chơi từ Dropdown (Team 2 hoặc Team 4)
//         string mode = dropdownInviteMode.options[dropdownInviteMode.value].text;
        
//         // 1. CHỦ PHÒNG TỰ CHẾ RA MỘT TÊN PHÒNG BÍ MẬT
//         string randomRoomName = "Room_" + myUser + "_" + Random.Range(1000, 9999);

//         // 2. BẮN TÊN PHÒNG ĐÓ LÊN SERVER ĐỂ GỬI CHO BẠN BÈ
//         StartCoroutine(SendInviteRoutine(myUser, targetUser, randomRoomName, mode));

//         // 3. CHỦ PHÒNG ĐI VÀO PHÒNG TRƯỚC VÀ ĐỨNG CHỜ
//         FindObjectOfType<GameUIManager>().HostRoomForFriend(randomRoomName, mode);
//     }

//     IEnumerator SendInviteRoutine(string sender, string receiver, string roomName, string mode) {
//         // Đảm bảo apiUrl của bạn đang là: "http://gameserver.runasp.net/api/Social"
//         string url = $"{apiUrl}/invite-party?senderName={sender}&receiverName={receiver}&roomName={roomName}&gameMode={mode}";
        
//         using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "")) {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success) {
//                 Debug.Log($"Đã ném thiệp mời cho {receiver}. Đang vào phòng chờ...");
//             } else {
//                 Debug.LogError("Lỗi gửi lời mời: " + www.downloadHandler.text);
//             }
//         }
//     }
//     // GỌI HÀM NÀY SAU KHI ĐĂNG NHẬP THÀNH CÔNG (Trong GameUIManager)
//     public void StartSocialFeatures() {
//         if (!isCheckingInvites) {
//             StartCoroutine(PingOnlineRoutine());
//             StartCoroutine(CheckInvitesRoutine());
//             isCheckingInvites = true;
//         }
//     }

//     // 1. TỰ ĐỘNG BÁO ONLINE MỖI 60 GIÂY
//     IEnumerator PingOnlineRoutine() {
//         while (true) {
//             string myUser = PlayerPrefs.GetString("SavedUser", "");
//             if (!string.IsNullOrEmpty(myUser)) {
//                 using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{apiUrl}/ping-online/{myUser}", "")) {
//                     yield return www.SendWebRequest();
//                 }
//             }
//             yield return new WaitForSeconds(60f); // Đợi 1 phút báo lại 1 lần
//         }
//     }

//     // 2. KẾT BẠN
//     void SendFriendRequest() {
//         string targetUser = inputFriendName.text;
//         string myUser = PlayerPrefs.GetString("SavedUser", "");
//         if (string.IsNullOrEmpty(targetUser)) return;

//         StartCoroutine(AddFriendRoutine(myUser, targetUser));
//     }

//     IEnumerator AddFriendRoutine(string sender, string receiver) {
//         // API: /api/Social/add-friend?senderName=...&receiverName=...
//         string url = $"{apiUrl}/add-friend?senderName={sender}&receiverName={receiver}";
//         using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "")) {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success) {
//                 Debug.Log("Đã gửi lời mời kết bạn!");
//                 inputFriendName.text = ""; // Xóa trắng ô nhập
//             } else {
//                 Debug.LogError("Lỗi kết bạn: " + www.downloadHandler.text);
//             }
//         }
//     }

//     // 3. TỰ ĐỘNG KIỂM TRA LỜI MỜI MỖI 3 GIÂY
//     IEnumerator CheckInvitesRoutine() {
//         while (true) {
//             string myUser = PlayerPrefs.GetString("SavedUser", "");
//             if (!string.IsNullOrEmpty(myUser) && !panelInvitePopup.activeSelf) {
                
//                 using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/check-invites/{myUser}")) {
//                     yield return www.SendWebRequest();

//                     if (www.result == UnityWebRequest.Result.Success && www.downloadHandler.text != "[]") {
//                         // Tạm thời xử lý chuỗi JSON thủ công cho nhanh (Vì JSON trả về dạng Array)
//                         string json = www.downloadHandler.text;
                        
//                         // Nếu có lời mời (JSON không rỗng) -> Bật Popup
//                         if (json.Contains("senderName")) {
//                             ParseAndShowInvite(json);
//                         }
//                     }
//                 }
//             }
//             yield return new WaitForSeconds(3f); // Cứ 3 giây quét 1 lần
//         }
//     }

//     void ParseAndShowInvite(string json) {
//         // Mẹo bóc tách JSON nhanh cho đồ án (Nếu dùng thư viện Newtonsoft.Json thì càng tốt)
//         // json có dạng: [{"id":1, "senderName":"Duy", "roomName":"Room_ABC", "gameMode":"Team 2"}]
        
//         // Trích xuất dữ liệu cơ bản
//         string sender = ExtractJsonString(json, "senderName");
//         pendingRoomName = ExtractJsonString(json, "roomName");
//         pendingGameMode = ExtractJsonString(json, "gameMode");

//         // Hiện Popup
//         txtInviteMessage.text = $"{sender} mời bạn vào chế độ {pendingGameMode}!";
//         panelInvitePopup.SetActive(true);
//     }

//     // Hàm phụ trợ bóc tách JSON siêu tốc
//     string ExtractJsonString(string json, string key) {
//         string search = "\"" + key + "\":\"";
//         int start = json.IndexOf(search) + search.Length;
//         int end = json.IndexOf("\"", start);
//         return json.Substring(start, end - start);
//     }

//     // 4. BẤM CHẤP NHẬN LỜI MỜI
//     void AcceptInvite() {
//         panelInvitePopup.SetActive(false);
//         Debug.Log($"Đang vào phòng {pendingRoomName} chế độ {pendingGameMode}...");
        
//         // GỌI HÀM VÀO PHÒNG BÊN GAMEUIMANAGER (Cần viết thêm bên GameUIManager)
//         FindObjectOfType<GameUIManager>().JoinRoomFromInvite(pendingRoomName, pendingGameMode);
//     }
// }