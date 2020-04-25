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
    public delegate void OnResponse(string response);
    public static event OnResponse OnFindGameResponseEvent;

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

    public void FindGameCallback(string response)
    {
        OnFindGameResponseEvent.Invoke(response);
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
}
