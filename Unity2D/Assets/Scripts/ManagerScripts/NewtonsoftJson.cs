using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

public class NewtonsoftJson
{
    private static NewtonsoftJson _instance;
    public static NewtonsoftJson Instance
    {
        get
        {
            if(_instance == null)
                _instance = new NewtonsoftJson();

            return _instance;
        }
    }

    public string ObjectToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public T JsonToObject<T>(string jsonData)
    {
        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    public void SaveJsonFile(string createPath, string fileName, string jsonData)
    {
        JObject jsonObj = JObject.Parse(jsonData);
        jsonData = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", createPath, fileName), FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }

    public void SaveJsonFileToStreamingAssets(string createPath, string fileName, string jsonData)
    {
        string savePath = Path.Combine(Application.persistentDataPath, createPath);
        if(!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
        string filePath = Path.Combine(savePath, $"{fileName}.json");

        // Create a new file or overwrite an existing file
        StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8);

        // Write the JSON data to the file
        writer.Write(jsonData);

        // Close the writer
        writer.Close();
    }




    public T LoadJsonFile<T>(string loadPath, string fileName)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", loadPath, fileName), FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string jsonData = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(jsonData);
    }
}
