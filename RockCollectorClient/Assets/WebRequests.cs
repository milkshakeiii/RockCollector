using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

public class WebRequests : MonoBehaviour
{
    private const string baseUrl = "https://polls-service-7527wqjyaq-uc.a.run.app/playapp/";
    private const string getAuthTokenUrl = baseUrl + "users_authenticate_or_create_from_steam";

    private string authToken = "";

    private static WebRequests instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static WebRequests GetInstance()
    {
        return instance;
    }

    public void GetAuthTokenViaSteamLogin(string sessionTicket)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        data.Add("steam_auth_ticket", sessionTicket);

        StartCoroutine(Post(getAuthTokenUrl, data, (response) =>
        {
            authToken = response.GetValue("token").ToString();
            Debug.Log("authToken: " + authToken);
        }));
    }

    public IEnumerator Post(string url, Dictionary<string, string> data, System.Action<Newtonsoft.Json.Linq.JObject> callback)
    {
        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> pair in data)
        {
            form.AddField(pair.Key, pair.Value);
        }
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // the response is a json string, so we need to convert it to a dictionary
                Newtonsoft.Json.Linq.JObject response = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(www.downloadHandler.text);
                callback(response);
            }
        }
    }
}
