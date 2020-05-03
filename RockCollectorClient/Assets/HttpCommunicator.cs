using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HttpCommunicator : MonoBehaviour
{
    private string username = "";
    private string wordpass = "";

    private static HttpCommunicator instance;

    public delegate void OnRequest();
    public static event OnRequest OnFindGameRequestEvent;
    public static event OnRequest OnCheckForGamestateRequestEvent;
    public static event OnRequest OnTakeTurnRequestEvent;
    public delegate void OnResponse(string response);
    public static event OnResponse OnFindGameResponseEvent;
    public static event OnResponse OnCheckForGamestateResponseEvent;
    public static event OnResponse OnTakeTurnResponseEvent;

    void Start()
    {
        instance = this;
    }

    public static HttpCommunicator GetInstance()
    {
        return instance;
    }

    public void FindGameRequest(string newUsername, string newWordpass)
    {
        username = newUsername;
        wordpass = newWordpass;

        OnFindGameRequestEvent.Invoke();
        StartCoroutine(DoRequest(new Dictionary<string, string>(), "webapi/find_game", FindGameCallback));
    }

    private void FindGameCallback(string response)
    {
        OnFindGameResponseEvent.Invoke(response);
    }

    public void CheckForGamestateRequest(string gameUuid, string turnsTaken)
    {
        if (OnCheckForGamestateRequestEvent != null)
            OnCheckForGamestateRequestEvent.Invoke();
        Dictionary<string, string> requestData = new Dictionary<string, string>();
        requestData["game_uuid"] = gameUuid;
        requestData["turns_taken"] = turnsTaken;
        StartCoroutine(DoRequest(requestData, "webapi/check_for_gamestate", CheckForGamestateCallback));
    }

    private void CheckForGamestateCallback(string response)
    {
        OnCheckForGamestateResponseEvent.Invoke(response);
    }

    public void TakeTurnRequest(string gameUuid,
                                string takingTurnNumber,
                                string sourceRockIndex,
                                string targetX,
                                string targetY)
    {
        if (OnTakeTurnRequestEvent != null)
            OnTakeTurnRequestEvent.Invoke();
        Dictionary<string, string> requestData = new Dictionary<string, string>();
        requestData["game_uuid"] = gameUuid;
        requestData["taking_turn_number"] = takingTurnNumber;
        requestData["source_rock_index"] = sourceRockIndex;
        requestData["target_x"] = targetX;
        requestData["targey_y"] = targetY;
        StartCoroutine(DoRequest(requestData, "webapi/make_move", TakeTurnCallback));
    }

    private void TakeTurnCallback(string response)
    {
        OnTakeTurnResponseEvent.Invoke(response);
    }

    IEnumerator DoRequest(Dictionary<string, string> formData, string endpoint, OnResponse callback)
    {
        if (username.Equals("") || wordpass.Equals(""))
            throw new UnityException("uername or wordpass not set");

        formData["username"] = username;
        formData["password"] = wordpass;

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/"+endpoint, formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("http response:");
            string response = www.downloadHandler.text;
            Debug.Log(response);
            callback(response);
        }
    }

    public static string BeforeTheColon(string line)
    {
        string[] split = line.Split(':');
        return split[0];
    }
}
