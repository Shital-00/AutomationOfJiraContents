
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class ScreenshotDownloader : MonoBehaviour //thumbnail downloader or door icon downloader
{
    //Jira instance details
    string baseUrl = "your jira domain";  //ex-https://your-domain.atlassian.net// Replace with your Jira URL
    string username = "your username";  //ex - shgu@ciklum.com// Replace with your Jira email
    /// <summary>
    /// /Note ex my API token ->
    /// </summary>
    string apiToken = "your API Token";
    string sprintId = "your sprint id"; //ex-705 // Active sprint id 
    string issueTypeToFilter = "your keyword to chow "; //ex - Game Story The issue type to filter for
    string keyword = "your keyword"; //ex - thumb // Keyword to search for in attachment filenames with thumbs
   // string keyword = "door"; //keyword to serach for attachment filenames with door
    private string targetBoardName = "your board name"; //ex -> IGN INFO - Delivery
    string boardID = "board-id"; // ex - IGN INFO-Delivery // 104

    void Start()
    {
        StartCoroutine(GetIssuesForSprint());
    }

    //Coroutine to fetch all issues in a sprint with pagination
    IEnumerator GetIssuesForSprint()
    {
        int startAt = 0;
        int maxResults = 50; // Adjust this based on your needs (maximum is typically 100)
        bool hasMoreIssues = true;

        while (hasMoreIssues)
        {
            string url = $"{baseUrl}/rest/agile/1.0/sprint/{sprintId}/issue?startAt={startAt}&maxResults={maxResults}";

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}")));

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching sprint issues: " + request.error);
                hasMoreIssues = false; // Stop fetching on error
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Sprint Issues Response: " + jsonResponse);

                IssueList issueList = JsonUtility.FromJson<IssueList>(jsonResponse);

                //Process the fetched issues
                if (issueList != null && issueList.issues != null)
                {
                    foreach (Issue issue in issueList.issues)
                    {
                        Debug.Log($"Processing issue {issue.key} with type: {issue.fields.issuetype?.name}");

                       // Check if the issue type matches the one we want
                        if (issue.fields.issuetype != null && issue.fields.issuetype.name == issueTypeToFilter)
                        {
                            Debug.Log("Found Game Story issue: " + issue.key);
                            StartCoroutine(CheckAttachmentsForIssue(issue));
                        }
                    }

                    //If the number of issues fetched is less than maxResults, we've reached the end
                    if (issueList.issues.Length < maxResults)
                    {
                        hasMoreIssues = false; // No more issues to fetch
                    }
                    else
                    {
                        startAt += maxResults; // Update startAt for the next set of issues
                    }
                }
                else
                {
                    hasMoreIssues = false; // No valid issues or empty response
                }
            }
        }
    }
   IEnumerator CheckAttachmentsForIssue(Issue issue)
    {
        string url = $"{baseUrl}/rest/api/2/issue/{issue.key}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}")));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching issue details: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Issue Details Response: " + jsonResponse);

            IssueDetails issueDetails = JsonUtility.FromJson<IssueDetails>(jsonResponse);
            string summary = issueDetails.fields.summary;

            if (issueDetails != null && issueDetails.fields != null && issueDetails.fields.attachment != null)
            {
                foreach (Attachment attachment in issueDetails.fields.attachment)
                {
                    if (attachment.filename.Contains(keyword))
                    {
                        StartCoroutine(DownloadAttachment(attachment.content, summary, attachment.filename));
                    }
                }
            }
        }
    }

    IEnumerator DownloadAttachment(string attachmentUrl, string headline, string filename)
    {
        UnityWebRequest request = UnityWebRequest.Get(attachmentUrl);
        request.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}")));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error downloading attachment: " + request.error);
        }
        else
        {
            string sanitizedSummary = SanitizeFileName(headline);
            string fileExtension = Path.GetExtension(filename);
            string newFileName = sanitizedSummary + fileExtension;
            string path = Path.Combine(Application.persistentDataPath, newFileName);
            System.IO.File.WriteAllBytes(path, request.downloadHandler.data);
            Debug.Log("Downloaded attachment to: " + path);
        }
    }

    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
   
}

//Jira Stories info
[System.Serializable]
public class IssueList
{
    public Issue[] issues;  // Array of issues in the sprint
}

//Jira Stories details
[System.Serializable]
public class Issue
{
    public string key;  // Issue key (e.g., "PROJ-123")
    public IssueFields fields;  // The issue fields, including attachments
}

//Issue fields including issue type
[System.Serializable]
public class IssueFields
{
    public Attachment[] attachment;  // Array of attachments
    public IssueType issuetype; // The issue type
    public string summary;
}

//Issue type details
[System.Serializable]
public class IssueType
{
    public string name;  // The issue type name (e.g., "Game Story")
}

//Attachments details
[System.Serializable]
public class Attachment
{
    public string filename;  // Filename of the attachment
    public string content;  // URL to download the attachment
}

//Issue details including attachments
[System.Serializable]
public class IssueDetails
{
    public IssueFields fields;  // Issue fields, including attachments
}







///////Getting Specific Board ID from Jira and Specific Sprint Id


//using System.Collections;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;

//public class ScreenshotDownloader : MonoBehaviour
//{
//   
//    void Start()
//    {
//        StartCoroutine(GetBoardIdByName(targetBoardName));
//    }

//    private IEnumerator GetBoardIdByName(string boardName)
//    {
//        bool boardFound = false;
//        int startAt = 0;
//        int maxResults = 50;

//        while (!boardFound)
//        {
//            string url = $"{jiraDomain}/rest/agile/1.0/board?startAt={startAt}&maxResults={maxResults}";

//            using (UnityWebRequest request = UnityWebRequest.Get(url))
//            {
//                // Set the authorization header
//                string auth = $"{username}:{apiToken}";
//                string encodedAuth = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
//                request.SetRequestHeader("Authorization", "Basic " + encodedAuth);
//                request.SetRequestHeader("Content-Type", "application/json");

//                // Send the request and wait for a response
//                yield return request.SendWebRequest();

//                // Check for errors
//                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
//                {
//                    Debug.LogError("Error fetching boards: " + request.error);
//                    yield break;
//                }

//                // Parse the JSON response
//                var jsonResponse = request.downloadHandler.text;
//                Debug.Log(jsonResponse);
//                BoardResponse boards = JsonUtility.FromJson<BoardResponse>(jsonResponse);

//                // Search for the board with the specified name
//                foreach (var board in boards.values)
//                {
//                    if (board.name.Equals(boardName))
//                    {
//                        //Debug.Log("Board ID: " + board.id);
//                        StartCoroutine(GetActiveSprintWithWordInName(board.id, targetSprintWord));
//                        yield break;
//                    }
//                }

//                // Check if we need to fetch the next page
//                if (!boardFound && boards.isLast == false)
//                {
//                    startAt += maxResults;
//                }
//                else
//                {
//                    break; // Exit if board is found or no more pages are available
//                }
//            }
//        }

//        if (!boardFound)
//        {
//            Debug.LogError("Board with the specified name not found.");
//        }
//    }


//    private IEnumerator GetActiveSprintWithWordInName(int boardId, string sprintWord)
//    {
//        string url = $"{jiraDomain}/rest/agile/1.0/board/{boardId}/sprint?state=active";
//        using (UnityWebRequest request = UnityWebRequest.Get(url))
//        {
//            string auth = $"{username}:{apiToken}";
//            string encodedAuth = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
//            request.SetRequestHeader("Authorization", "Basic " + encodedAuth);
//            request.SetRequestHeader("Content-Type", "application/json");

//            yield return request.SendWebRequest();

//            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
//            {
//                Debug.LogError("Error fetching sprints: " + request.error);
//            }
//            else
//            {
//                var jsonResponse = request.downloadHandler.text;
//                SprintResponse sprints = JsonUtility.FromJson<SprintResponse>(jsonResponse);

//                bool sprintFound = false;
//                foreach (var sprint in sprints.values)
//                {
//                    if (sprint.name.Contains(sprintWord))
//                    {
//                        Debug.Log("Active Sprint ID with specified word in name: " + sprint.id);
//                        sprintFound = true;
//                        break;
//                    }
//                }

//                if (!sprintFound)
//                {
//                    Debug.LogError("No active sprint with specified word found in name.");
//                }
//            }
//        }
//    }

//    // JSON data classes for parsing with JsonUtility
//    [System.Serializable]
//    private class BoardResponse
//    {
//        public Board[] values;
//        public bool isLast;
//    }

//    [System.Serializable]
//    private class Board
//    {
//        public int id;
//        public string name;
//        public string type;
//    }

//    [System.Serializable]
//    private class SprintResponse
//    {
//        public Sprint[] values;
//    }

//    [System.Serializable]
//    private class Sprint
//    {
//        public int id;
//        public string name;
//        public string state;
//    }
//}
