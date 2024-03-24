using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Azure.Data.Tables;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using TMPro;
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class CheckForUpdatesService : MonoBehaviour
{

    public GameObject _buttonCheckForUpdates;
    public GameObject _inputFieldUsername;
    public GameObject _configurationsLabel;
    public AudioSource AudioSource;
    private Settings _currentDefaultConfiguration;
    private CloudStorageAccount _cloudStorageAccount;

    string storageConnectionString = "REMOVED";
    string tableName = "usergameconfiguration";

    public CheckForUpdatesService()
    {
        _cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
    }

    public async void Start()
    {
        try
        {
            Debug.Log("About to try and load configuration from central server.");

            await LoadLatestConfigFromServer();

            Debug.Log("Successfully loaded central configuration.");
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }


    public void PlaySoundEffect()
    {
        try
        {
            AudioSource.Play();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    public async void CheckForUpdates()
    {
        try
        {
            await GetUsersConfigFromServerAndDownloadFile();
            await UpsertUserConfig();
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    private async Task LoadLatestConfigFromServer()
    {
        var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference("centralisedfiles");
        var blob = container.GetBlobReference("settings.json");
        Stream stream = new MemoryStream();

        await blob.DownloadToStreamAsync(stream);

        //using will dispose of object meaning it will be marked as such and 
        //when the GC next runs it will be removed from memory
        using (StreamReader reader = new StreamReader(stream))
        {
            stream.Seek(0, SeekOrigin.Begin);
            string settingsAsString = reader.ReadToEnd();
            _currentDefaultConfiguration = JsonConvert.DeserializeObject<Settings>(settingsAsString);

            if(_configurationsLabel != null)
                _configurationsLabel.GetComponent<TMP_Text>().text = settingsAsString;  

            Debug.Log($"Settings On the Server:{settingsAsString}");
        }
    }

    private async Task GetUsersConfigFromServerAndDownloadFile()
    {
        await GetCentralisedUserConfig();

        //for is more performant on larger data sets, however this will stream the
        //data rather than butter it.
        foreach (var audioFile in _currentDefaultConfiguration.AudioFiles)
        {
            string localLocation = await DownloadFileAndWriteToAssets(audioFile);

            //Coroutine appears to be a wrapper object for yielding and the progression 
            //of the next item returned
            StartCoroutine(LoadMusic(localLocation));
        }
    }



    private async Task UpsertUserConfig()
    {
        Debug.Log($"About to try and update config for user {this.Username}");

        TableClient tableClient = new TableClient(storageConnectionString, tableName);

        var testInsertObject = new Azure.Data.Tables.TableEntity("Uk", this.Username) 
        { 
            { "Version", _currentDefaultConfiguration.Version  } 
        };

        await tableClient.UpsertEntityAsync(testInsertObject);

        Debug.Log($"Successfully updated config for user {this.Username}");
    }

    private async Task GetCentralisedUserConfig()
    {
        Debug.Log("About to try and load centralised config.");

        CloudTableClient tableClient = _cloudStorageAccount.CreateCloudTableClient();
        List<string> columns = new List<string>() { "Version" };

        var table = tableClient.GetTableReference("usergameconfiguration");
        var tableOperation = TableOperation.Retrieve("Uk", this.Username, columns);
        var userConfigs = await table.ExecuteAsync(tableOperation);

        Debug.Log("Succesfully loaded centralised config.");
    }

    //Using ValueTask for a value type task to save memory allocations negatices of a ValueTask of only being 
    //able to be called once is not a problem.
    private async ValueTask<string> DownloadFileAndWriteToAssets(AudioFile audioFile)
    {
        Debug.Log("About to try and download the audio file from blob storage.");

        var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference("centralisedfiles");
        var blob = container.GetBlobReference(audioFile.Name);
        Stream stream = new MemoryStream();

        await blob.DownloadToStreamAsync(stream);

        using (StreamReader reader = new StreamReader(stream))
        {
            stream.Seek(0, SeekOrigin.Begin);

            string assetsLocation = $"{Application.persistentDataPath}/{audioFile.Name}";
            FileStream fileStream = new FileStream(assetsLocation, FileMode.Create);

            await stream.CopyToAsync(fileStream);

            Debug.Log("Finished writing file to local disk.");

            return assetsLocation;
        }
    }

    private IEnumerator LoadMusic(string filePath)
    {
        Debug.Log("About to try and set local audio file to AudioSource.");

        using (var unityWebRequestMultiMedia = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
        {
            var downloadHandlerAudioClip = ((DownloadHandlerAudioClip)unityWebRequestMultiMedia.downloadHandler);
            downloadHandlerAudioClip.streamAudio = true;

            yield return unityWebRequestMultiMedia.SendWebRequest();

            if (unityWebRequestMultiMedia.result == UnityWebRequest.Result.ConnectionError)
            {
                //Yield will return to the IEnumerator and stops you having to 
                Debug.LogError(unityWebRequestMultiMedia.error);
                yield break;
            }

            if (downloadHandlerAudioClip.isDone)
                SetNewFileToAudioSource(unityWebRequestMultiMedia, downloadHandlerAudioClip);

            Debug.Log("Finished setting audio clip.");
        }

    }

    private void SetNewFileToAudioSource(UnityWebRequest unityWebRequestMultiMedia, DownloadHandlerAudioClip downloadHandlerAudioClip)
    {
        AudioClip audioClip = downloadHandlerAudioClip.audioClip;

        if (audioClip != null)
        {
            audioClip = DownloadHandlerAudioClip.GetContent(unityWebRequestMultiMedia);
            AudioSource.clip = audioClip;
        }
    }

    private string Username => this._inputFieldUsername.GetComponent<TMP_InputField>().text;

}
