using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEngine.XR;
using UnityEngine.InputSystem;

public class FileManager : MonoBehaviour
{
    public Button sourseFolder;
    public Button destinationFolder;
    public Button renameBtn;
    private string soursePath;
    private string destinationPath;
    //public TMP_InputField text;
    private void Start()
    {
        sourseFolder.onClick.AddListener(() => OpenSourseFolder());
        destinationFolder.onClick.AddListener(() => OpenDestinationFolder());
        renameBtn.onClick.AddListener(() => RenameFiles()); 
    }

    public void OpenSourseFolder()
    {
        soursePath = EditorUtility.OpenFolderPanel("Select your Files", "", "");
        Debug.Log(soursePath);
    }
    public void OpenDestinationFolder()
    {
        destinationPath = EditorUtility.OpenFolderPanel("Select your Destination Folder", "", "");
        Debug.Log(destinationPath);
    }
    public void RenameFiles()
    {
        DirectoryInfo Source = new DirectoryInfo(soursePath);
        DirectoryInfo Destination = new DirectoryInfo(destinationPath);
        FileInfo[] file = Source.GetFiles("*.png");

        //Debug.Log("SorcePath" + soursePath);
        //Debug.Log("Destination" + destinationPath);
        //Debug.Log("gfdf" + Source.ToString());
        //Debug.Log("dfscx " + Destination.ToString());

        foreach (FileInfo files in file)
        {
            Regex pattern = new Regex(@"\d+");
            Match match = pattern.Match(files.Name);
            int number = int.Parse(match.Value);
            string digit = number.ToString();
            if (digit.Length < 3)
            {
                char zero = '0';
                File.Move(files.FullName, Destination.ToString()+ "//gp_game_" + digit.PadLeft(3, zero) + "_screenshot"+ files.Extension);
            }
            else
            {
                File.Move(files.FullName, Destination.ToString() + "//gp_game_" + digit + "_screenshot" + files.Extension);
            }

        }
    }
}
