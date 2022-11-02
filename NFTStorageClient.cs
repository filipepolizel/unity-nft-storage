using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Text;
using NFTStorage.JSONSerialization;
using UnityEngine.Networking;

namespace NFTStorage.JSONSerialization
{
    [Serializable]
    public class NFTStorageError
    {
        public string name;
        public string message;
    }

    [Serializable]
    public class NFTStorageFiles
    {
        public string name;
        public string type;
    }

    [Serializable]
    public class NFTStorageDeal
    {
        public string batchRootCid;
        public string lastChange;
        public string miner;
        public string network;
        public string pieceCid;
        public string status;
        public string statusText;
        public int chainDealID;
        public string dealActivation;
        public string dealExpiration;
    }

    [Serializable]
    public class NFTStoragePin
    {
        public string cid;
        public string name;
        public string status;
        public string created;
        public int size;
        // TODO: add metadata parsing ('meta' property)
    }

    [Serializable]
    public class NFTStorageNFTObject
    {
        public string cid;
        public int size;
        public string created;
        public string type;
        public string scope;
        public NFTStoragePin pin;
        public NFTStorageFiles[] files;
        public NFTStorageDeal[] deals;
    }


    [Serializable]
    public class NFTStorageCheckValue
    {
        public string cid;
        public NFTStoragePin pin;
        public NFTStorageDeal[] deals;
    }

    [Serializable]
    public class NFTStorageGetFileResponse
    {
        public bool ok;
        public NFTStorageNFTObject value;
        public NFTStorageError error;
    }

    [Serializable]
    public class NFTStorageCheckResponse
    {
        public bool ok;
        public NFTStorageCheckValue value;
        public NFTStorageError error;
    }

    [Serializable]
    public class NFTStorageListFilesResponse
    {
        public bool ok;
        public NFTStorageNFTObject[] value;
        public NFTStorageError error;
    }

    [Serializable]
    public class NFTStorageUploadResponse
    {
        public bool ok;
        public NFTStorageNFTObject value;
        public NFTStorageError error;
    }

    [Serializable]
    public class NFTStorageDeleteResponse
    {
        public bool ok;
    }
}

namespace NFTStorage
{
    // This is the main class for communicating with nft.storage and IPFS
    public class NFTStorageClient : MonoBehaviour
    {
        // nft.storage API endpoint
        private static readonly string nftStorageApiUrl = "https://api.nft.storage/";

        // HTTP client to communicate with nft.storage
        private static readonly HttpClient nftClient = new HttpClient();

        // http client to communicate with IPFS API
        private static readonly HttpClient ipfsClient = new HttpClient();

        // nft.storage API key
        public string apiToken;

        /**
        <summary>"Start" is called before the first frame update for initializing "NFTStorageClient"</summary>
        */
        void Start()
        {
            nftClient.DefaultRequestHeaders.Add("Accept", "application/json");
            if (apiToken != null)
            {
                nftClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiToken);
            }
            else
            {
                // log in console in case no API key is found during initialization
                Debug.Log("Starting NFT Storage Client without API key, please call 'SetApiToken' method before using class methods.");
            }
        }

        /**
        <summary>Auxiliar function for making a HTTP/HTTPS request</summary>
        <param name="method">The HTTP method to be used for the request (e.g. GET, POST, DELETE)</param>
        <param name="uri">The full URI for the request (including paths and query string parameters)</param>
        <param name="requestClient">The client to be used for the request (e.g. nft.storage, IPFS)</param>
        <returns>A "Task" which result is a string, containing the raw response data</returns>
        */
        private async Task<string> SendHttpRequest(HttpMethod method, string uri, HttpClient requestClient = null)
        {
            try
            {
                if (requestClient == null)
                {
                    requestClient = nftClient;
                }
                HttpRequestMessage request = new HttpRequestMessage(method, uri);
                HttpResponseMessage response = await requestClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Debug.Log("HTTP Request Exception: " + e.Message);
                return null;
            }
        }

        /**
        <summary>Auxiliar function for uploading a file via HTTP request</summary>
        <param name="uri">The HTTP API endpoint</param>
        <param name="paramString">The data to be uploaded, formatted as a string</param>
        <returns>A "Task" which result is a string, containing the raw response data</returns>
        */
        private async Task<string> Upload(string uri, string pathFile)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(pathFile);
            try
            {
                    using (var content = new ByteArrayContent(bytes))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("*/*");

                        nftClient.Timeout = new TimeSpan(1, 0, 0);// 1 hour should be enough probably

                        //Send it
                        print("Uploading...");
                        var response = await nftClient.PostAsync(uri, content);
                        response.EnsureSuccessStatusCode();
                        Stream responseStream = await response.Content.ReadAsStreamAsync();
                        StreamReader reader = new StreamReader(responseStream);
                        return reader.ReadToEnd();
                    }
            }
            catch (HttpRequestException e)
            {
                Debug.Log("HTTP Request Exception: " + e.Message);
                return null;
            }
        }




        UnityWebRequest uwr;

        IEnumerator PostRequest(string url, string filePath)
        {
            byte[] dataToPost = System.IO.File.ReadAllBytes(filePath);
            UploadHandlerRaw uhr = new UploadHandlerRaw(dataToPost);

            uwr = new UnityWebRequest(url, "POST", new DownloadHandlerBuffer(), uhr);
            uwr.SetRequestHeader("Accept", "application/json");
            uwr.SetRequestHeader("Authorization", "Bearer " + apiToken);
            uwr.SetRequestHeader("Content-Type", "*/*");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received: " + uwr.downloadHandler.text);
            }
        }

        IEnumerator UploadProgressCoroutine()
        {
            while (!uwr.isDone)
            {
                HandleProgress(uwr.uploadProgress);
                yield return null;
            }
        }

        void HandleProgress(float currentProgress)
        {
            print(currentProgress); // upload progrees bettween 0 and 1 
        }




        /**
        <summary>Set API token to be used as authorization header for "nft.storage" HTTP API requests</summary>
        <param name="token">A valid API key to be used with "nft.storage" API</param>
        */
        public void SetApiToken(string token)
        {
            apiToken = token;
            if (nftClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                nftClient.DefaultRequestHeaders.Remove("Authorization");
            }
            nftClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiToken);
        }

        /**
        <summary>Retrieve details of files that were previously uploaded using "nft.storage" HTTP API</summary>
        <param name="before">An ISO 8601 date/time string, which is used to filter solely files stored previously to this date/time</param>
        <param name="limit">An integer indicating the maximum number of files that should be retrieved</param>
        <returns>A "Task" which result is a "NFTStorageListFilesResponse" object, obtained by parsing JSON from "nft.storage" API response (GET / endpoint)</returns>
        */
        public async Task<NFTStorageListFilesResponse> ListFiles(string before = null, int limit = 10)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (before != null) query["before"] = before;
            query["limit"] = limit.ToString();
            string queryString = query.ToString();
            string requestUri = nftStorageApiUrl + "?" + queryString;
            string rawResponse = await SendHttpRequest(HttpMethod.Get, requestUri);
            NFTStorageListFilesResponse parsedResponse = JsonUtility.FromJson<NFTStorageListFilesResponse>(rawResponse);
            return parsedResponse;
        }

        /**
        <summary>Get details of a previously uploaded file using "nft.storage" HTTP API</summary>
        <param name="cid">The alphanumeric hash (base32) which uniquely identifies the file stored in IPFS system</param>
        <returns>A "Task" which result is a  "NFTStorageGetFileResponse" object, obtained by parsing JSON from "nft.storage" API response (GET /[cid] endpoint)</returns>
        */
        public async Task<NFTStorageGetFileResponse> GetFile(string cid)
        {
            string requestUri = nftStorageApiUrl + "/" + cid;
            string rawResponse = await SendHttpRequest(HttpMethod.Get, requestUri);
            NFTStorageGetFileResponse parsedResponse = JsonUtility.FromJson<NFTStorageGetFileResponse>(rawResponse);
            return parsedResponse;
        }

        /**
        <summary>Download data for specific file stored via "nft.storage" (using IPFS)</summary>
        <param name="cid">The alphanumeric hash (base32) which uniquely identifies the file stored in IPFS system</param>
        <returns>A string containing all data from the file, in its raw format</returns>
        */
        public async Task<string> GetFileData(string cid)
        {
            string requestUri = "https://" + cid + ".ipfs.dweb.link/";
            string response = await SendHttpRequest(HttpMethod.Get, requestUri, ipfsClient);
            return response;
        }

        /**
        <summary>Check a previously uploaded file using "nft.storage" HTTP API</summary>
        <param name="cid">The alphanumeric hash (base32) which uniquely identifies the file stored in IPFS system</param>
        <returns>A "Task" which result is a "NFTStorageCheckResponse" object, obtained by parsing JSON from "nft.storage" API response (GET /check endpoint)</returns>
        */
        public async Task<NFTStorageCheckResponse> CheckFile(string cid)
        {
            string requestUri = nftStorageApiUrl + "/check/" + cid;
            string rawResponse = await SendHttpRequest(HttpMethod.Get, requestUri);
            NFTStorageCheckResponse parsedResponse = JsonUtility.FromJson<NFTStorageCheckResponse>(rawResponse);
            return parsedResponse;
        }

        /**
        <summary>Delete a previously uploaded file using "nft.storage" HTTP API</summary>
        <param name="cid">The alphanumeric hash (base32) which uniquely identifies the file stored in IPFS system</param>
        <returns>A "Task" which result is a "NFTStorageDeleteResponse" object, obtained by parsing JSON from "nft.storage" API response (DELETE / endpoint)</returns>
        */
        public async Task<NFTStorageDeleteResponse> DeleteFile(string cid)
        {
            string requestUri = nftStorageApiUrl + "/" + cid;
            string rawResponse = await SendHttpRequest(HttpMethod.Delete, requestUri);
            NFTStorageDeleteResponse parsedResponse = JsonUtility.FromJson<NFTStorageDeleteResponse>(rawResponse);
            return parsedResponse;
        }

        /**
        <summary>Upload a file using "nft.storage" HTTP API</summary>
        <returns>A "Task" which result is a "NFTStorageUploadResponse" object, obtained by parsing JSON from "nft.storage" API response (POST /upload endpoint)</returns>
        */
        public async Task<NFTStorageUploadResponse> UploadDataFromStringHttpClient(string path)
        {
            string requestUri = nftStorageApiUrl + "/upload";
            string rawResponse = await Upload(requestUri, path);
            NFTStorageUploadResponse parsedResponse = JsonUtility.FromJson<NFTStorageUploadResponse>(rawResponse);
            return parsedResponse;
        }
        
        /*
        <summary>Upload a file using "nft.storage" unitywebrequest </summary>
        */
        
        public void UploadDataFromStringUnityWebrequest(string path)
        {
            string requestUri = nftStorageApiUrl + "/upload";
            uwr = null;
            StartCoroutine(PostRequest(requestUri, path));
            StartCoroutine(UploadProgressCoroutine());
        }
    }
}
