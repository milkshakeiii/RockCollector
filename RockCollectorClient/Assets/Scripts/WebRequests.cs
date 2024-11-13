using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

public class WebRequests : MonoBehaviour
{
    private const string baseUrl = "https://polls-service-7527wqjyaq-uc.a.run.app/playapp/";
    private const string getAuthTokenUrl = baseUrl + "users_authenticate_or_create_from_steam";
    private const string getPricesUrl = baseUrl + "get_market_prices";
    private const string reportPurchaseUrl = baseUrl + "report_purchase";
    private const string reportScoreUrl = baseUrl + "report_score";
    private const string getScoresUrl = baseUrl + "get_scores";

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

    public void ReportScore(List<string> scoreGroups, Dictionary<string, float> scores, string name, System.Action<Newtonsoft.Json.Linq.JObject> callback)
    {
        // create a json object with the scoreGroups and scores
        Newtonsoft.Json.Linq.JObject data = new()
        {
            { "groups", new Newtonsoft.Json.Linq.JArray(scoreGroups) },
            { "name", name }
        };
        foreach (KeyValuePair<string, float> score in scores)
        {
            data.Add(score.Key, score.Value);
        }

        StartCoroutine(Post(reportScoreUrl, data, callback));
    }

    public void GetScores(List<string> scoreGroups, string scoreType, int count, System.Action<Newtonsoft.Json.Linq.JObject> callback, int start = -1)
    {
        // create a json object with the scoreGroups, scoreType, count, and start if start is not -1
        Newtonsoft.Json.Linq.JObject data = new()
        {
            { "groups", new Newtonsoft.Json.Linq.JArray(scoreGroups) },
            { "score_type", scoreType },
            { "count", count }
        };
        if (start != -1)
        {
            data.Add("start", start);
        }

        StartCoroutine(Post(getScoresUrl, data, callback));
    }

    public void GetPrices(List<string> items, List<string> priceGroups, System.Action<Newtonsoft.Json.Linq.JObject> callback)
    {
        // create a json object with the items and priceGroups
        Newtonsoft.Json.Linq.JObject data = new()
        {
            { "items", new Newtonsoft.Json.Linq.JArray(items) },
            { "price_groups", new Newtonsoft.Json.Linq.JArray(priceGroups) }
        };

        StartCoroutine(Post(getPricesUrl, data, callback));
    }

    public void ReportPurchase(string item, string priceGroup, System.Action<Newtonsoft.Json.Linq.JObject> callback, bool isSale)
    {
        // create a json object with the item and priceGroup
        Newtonsoft.Json.Linq.JObject data = new()
        {
            { "item", item },
            { "price_group", priceGroup },
            { "price_adjustment", isSale ? -0.1 : 0.1 }
        };

        StartCoroutine(Post(reportPurchaseUrl, data, callback));
    }

    public void GetAuthTokenViaSteamLogin(string sessionTicket)
    {
        // create a json object with the sessionTicket
        Newtonsoft.Json.Linq.JObject data = new()
        {
            { "steam_auth_ticket", sessionTicket }
        };

        StartCoroutine(Post(getAuthTokenUrl, data, (response) =>
        {
            authToken = response.GetValue("token").ToString();
            Debug.Log("authToken: " + authToken);
        }));
    }

    public IEnumerator Post(string url, Newtonsoft.Json.Linq.JObject data, System.Action<Newtonsoft.Json.Linq.JObject> callback)
    {
        if (url != getAuthTokenUrl)
        {
            // wait for the authToken to be set
            while (authToken == "")
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        using UnityWebRequest www = UnityWebRequest.Post(url, data.ToString(), "application/json");
        if (url != getAuthTokenUrl)
        {
            www.SetRequestHeader("Authorization", "Token " + authToken);
        }
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // the response is a json string, so we need to convert it to a dictionary
            Newtonsoft.Json.Linq.JObject response = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(www.downloadHandler.text);
            if (response.GetValue("error") != null)
            {
                Debug.Log("Error: " + response.GetValue("error").ToString());
            }
            callback(response);
        }
    }
}
