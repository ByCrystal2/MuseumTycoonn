using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FirebaseWebGL.Scripts.FirebaseBridge; // Namespace for FirebaseWebGL Bridge

public class FirestoreManager : MonoBehaviour
{
    // Singleton instance
    public static FirestoreManager instance { get; private set; }

    // TaskCompletionSources for async operations
    private TaskCompletionSource<string> getUserTCS;
    private TaskCompletionSource<string> createUserTCS;
    private TaskCompletionSource<string> createGameDataTCS;
    private TaskCompletionSource<Dictionary<string, object>> getGameDataTCS;
    private TaskCompletionSource<bool> updateGameLangTCS;

    // Temporarily store userId for query
    private string queryUserId;

    private void Awake()
    {
        // Setup singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure a consistent GameObject name for callbacks (needed for SendMessage from JS)
        gameObject.name = "FirestoreManager";
    }

    /// <summary>
    /// Checks if a user exists by userId, creates a new user document if not,
    /// and ensures a GameDatas document under the user. Returns the user document ID.
    /// </summary>
    public async Task<string> GetOrCreateUserAsync(string userId, string userEmail, string userTelNo, string userName)
    {
        // Attempt to find existing user document by userId
        string userDocId = await GetUserDocumentIdByUserID(userId);
        if (string.IsNullOrEmpty(userDocId))
        {
            // User not found; create new user document
            userDocId = await CreateUserDocumentAsync(userId, userEmail, userTelNo, userName);
            // After creating user, create a GameDatas subcollection document
            await CreateUserGameDataDocAsync(userDocId);
        }
        return userDocId;
    }

    /// <summary>
    /// Retrieves the user's GameDatas document fields as a dictionary (including a "_docId" entry for the document ID).
    /// </summary>
    public async Task<Dictionary<string, object>> GetUserGameDataAsync(string userId)
    {
        // Get user document ID by userId
        string userDocId = await GetUserDocumentIdByUserID(userId);
        if (string.IsNullOrEmpty(userDocId))
            return null;

        // Prepare to get game data
        getGameDataTCS = new TaskCompletionSource<Dictionary<string, object>>();

        // Call Firestore to get documents in subcollection "GameDatas"
        FirebaseFirestore.GetDocumentsInCollection($"Users/{userDocId}/GameDatas",
            gameObject.name, "OnGetUserGameDataSuccess", "OnGetUserGameDataError");

        // Wait for callback and get result dictionary
        Dictionary<string, object> gameData = await getGameDataTCS.Task;
        return gameData;
    }

    /// <summary>
    /// Updates only the "GameLanguage" field in the user's GameDatas document.
    /// </summary>
    public async Task UpdateGameLanguageAsync(string userId, object newLanguage)
    {
        // Find the user document ID
        string userDocId = await GetUserDocumentIdByUserID(userId);
        if (string.IsNullOrEmpty(userDocId))
            throw new Exception("User document not found for userID: " + userId);

        // Get the GameDatas document (including its document ID in returned dictionary)
        Dictionary<string, object> gameData = await GetUserGameDataAsync(userId);
        if (gameData == null || !gameData.ContainsKey("_docId"))
            throw new Exception("GameDatas document not found for userID: " + userId);
        string gameDataDocId = gameData["_docId"].ToString();

        // Prepare JSON for update (only the GameLanguage field)
        Dictionary<string, object> updateDict = new Dictionary<string, object>
        {
            { "GameLanguage", newLanguage }
        };
        string jsonUpdate = DictionaryToJson(updateDict);

        updateGameLangTCS = new TaskCompletionSource<bool>();

        // Call Firestore to update the document field
        FirebaseFirestore.UpdateDocument($"Users/{userDocId}/GameDatas",
            gameDataDocId, jsonUpdate, gameObject.name, "OnUpdateGameDataSuccess", "OnUpdateGameDataError");

        // Wait for callback completion
        await updateGameLangTCS.Task;
    }

    /// <summary>
    /// Helper to get the Firestore document ID for a user by userId field.
    /// Returns empty string if not found.
    /// </summary>
    private async Task<string> GetUserDocumentIdByUserID(string userId)
    {
        queryUserId = userId;
        getUserTCS = new TaskCompletionSource<string>();

        // Get all documents in "Users" collection
        FirebaseFirestore.GetDocumentsInCollection("Users", gameObject.name, "OnGetUsersSuccess", "OnGetUsersError");

        // Wait for callback and result (docId or empty)
        string docId = await getUserTCS.Task;
        return docId;
    }

    /// <summary>
    /// Creates a new user document in "Users" collection with provided fields.
    /// Returns the new document ID.
    /// </summary>
    private async Task<string> CreateUserDocumentAsync(string userId, string userEmail, string userTelNo, string userName)
    {
        createUserTCS = new TaskCompletionSource<string>();

        // Prepare user data dictionary
        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "userID", userId },
            { "userEmail", userEmail },
            { "userTelNo", userTelNo },
            { "userName", userName },
            { "userSignInDate", DateTime.UtcNow.ToString("o") } // ISO 8601 format
        };
        string jsonUser = DictionaryToJson(userData);

        // Create document in "Users" with autogenerated ID
        FirebaseFirestore.AddDocument("Users", jsonUser, gameObject.name, "OnCreateUserSuccess", "OnCreateUserError");

        // Wait for callback to get new doc ID
        string newDocId = await createUserTCS.Task;
        return newDocId;
    }

    /// <summary>
    /// Creates a new document in the "GameDatas" subcollection for a given user document ID.
    /// Returns the new game data document ID.
    /// </summary>
    private async Task<string> CreateUserGameDataDocAsync(string userDocId)
    {
        createGameDataTCS = new TaskCompletionSource<string>();

        // Prepare initial game data (e.g. default GameLanguage)
        Dictionary<string, object> gameData = new Dictionary<string, object>
        {
            { "GameLanguage", "EN" }
        };
        string jsonGameData = DictionaryToJson(gameData);

        // Add document to subcollection "GameDatas" under the user document
        FirebaseFirestore.AddDocument($"Users/{userDocId}/GameDatas", jsonGameData, gameObject.name,
            "OnCreateGameDataSuccess", "OnCreateGameDataError");

        // Wait for callback to get new game data doc ID
        string newGameDataDocId = await createGameDataTCS.Task;
        return newGameDataDocId;
    }

    #region FirebaseWebGL Bridge Callback Handlers

    // Called when GetDocumentsInCollection("Users", ...) succeeds.
    private void OnGetUsersSuccess(string json)
    {
        try
        {
            string foundDocId = string.Empty;

            // Parse JSON response to find user with matching userID
            var data = JsonParser.Parse(json) as Dictionary<string, object>;
            if (data != null && data.ContainsKey("documents"))
            {
                var documents = data["documents"] as List<object>;
                if (documents != null)
                {
                    foreach (var docObj in documents)
                    {
                        var doc = docObj as Dictionary<string, object>;
                        if (doc != null && doc.ContainsKey("fields") && doc.ContainsKey("name"))
                        {
                            var fields = doc["fields"] as Dictionary<string, object>;
                            if (fields != null && fields.ContainsKey("userID"))
                            {
                                var userIdField = fields["userID"] as Dictionary<string, object>;
                                if (userIdField != null && userIdField.ContainsKey("stringValue"))
                                {
                                    string idVal = userIdField["stringValue"] as string;
                                    if (idVal == queryUserId)
                                    {
                                        // Found the matching user document
                                        string fullName = doc["name"] as string;
                                        foundDocId = ExtractDocumentIdFromName(fullName);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // Set result (either foundDocId or empty if not found)
            if (getUserTCS != null && !getUserTCS.Task.IsCompleted)
            {
                getUserTCS.SetResult(foundDocId);
            }
        }
        catch (Exception e)
        {
            if (getUserTCS != null)
                getUserTCS.SetException(e);
        }
    }

    // Called if GetDocumentsInCollection("Users", ...) fails.
    private void OnGetUsersError(string error)
    {
        if (getUserTCS != null && !getUserTCS.Task.IsCompleted)
            getUserTCS.SetException(new Exception(error));
    }

    private void OnCreateUserSuccess(string name)
    {
        // 'name' should be the full document path or doc ID
        string docId = ExtractDocumentIdFromName(name);
        if (createUserTCS != null && !createUserTCS.Task.IsCompleted)
            createUserTCS.SetResult(docId);
    }

    private void OnCreateUserError(string error)
    {
        if (createUserTCS != null && !createUserTCS.Task.IsCompleted)
            createUserTCS.SetException(new Exception(error));
    }

    private void OnCreateGameDataSuccess(string name)
    {
        string docId = ExtractDocumentIdFromName(name);
        if (createGameDataTCS != null && !createGameDataTCS.Task.IsCompleted)
            createGameDataTCS.SetResult(docId);
    }

    private void OnCreateGameDataError(string error)
    {
        if (createGameDataTCS != null && !createGameDataTCS.Task.IsCompleted)
            createGameDataTCS.SetException(new Exception(error));
    }

    // Called when GetDocumentsInCollection("Users/{user}/GameDatas", ...) succeeds.
    private void OnGetUserGameDataSuccess(string json)
    {
        try
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            var data = JsonParser.Parse(json) as Dictionary<string, object>;
            if (data != null && data.ContainsKey("documents"))
            {
                var documents = data["documents"] as List<object>;
                if (documents != null && documents.Count > 0)
                {
                    // Take first document in GameDatas subcollection
                    var doc = documents[0] as Dictionary<string, object>;
                    if (doc != null)
                    {
                        // Extract docId from name and include in result
                        if (doc.ContainsKey("name"))
                        {
                            string name = doc["name"] as string;
                            string docId = ExtractDocumentIdFromName(name);
                            result["_docId"] = docId;
                        }
                        // Extract all fields into result dictionary
                        if (doc.ContainsKey("fields"))
                        {
                            var fields = doc["fields"] as Dictionary<string, object>;
                            if (fields != null)
                            {
                                foreach (var kv in fields)
                                {
                                    var fieldValue = kv.Value as Dictionary<string, object>;
                                    if (fieldValue != null)
                                    {
                                        if (fieldValue.ContainsKey("stringValue"))
                                            result[kv.Key] = fieldValue["stringValue"];
                                        else if (fieldValue.ContainsKey("integerValue"))
                                            result[kv.Key] = Convert.ToInt64(fieldValue["integerValue"]);
                                        else if (fieldValue.ContainsKey("doubleValue"))
                                            result[kv.Key] = Convert.ToDouble(fieldValue["doubleValue"]);
                                        else if (fieldValue.ContainsKey("booleanValue"))
                                            result[kv.Key] = Convert.ToBoolean(fieldValue["booleanValue"]);
                                        // Extend with other Firestore types if needed
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (getGameDataTCS != null && !getGameDataTCS.Task.IsCompleted)
                getGameDataTCS.SetResult(result);
        }
        catch (Exception e)
        {
            if (getGameDataTCS != null)
                getGameDataTCS.SetException(e);
        }
    }

    private void OnGetUserGameDataError(string error)
    {
        if (getGameDataTCS != null && !getGameDataTCS.Task.IsCompleted)
            getGameDataTCS.SetException(new Exception(error));
    }

    private void OnUpdateGameDataSuccess(string result)
    {
        if (updateGameLangTCS != null && !updateGameLangTCS.Task.IsCompleted)
            updateGameLangTCS.SetResult(true);
    }

    private void OnUpdateGameDataError(string error)
    {
        if (updateGameLangTCS != null && !updateGameLangTCS.Task.IsCompleted)
            updateGameLangTCS.SetException(new Exception(error));
    }

    #endregion

    /// <summary>
    /// Extracts the Firestore document ID from a full document path string.
    /// </summary>
    private string ExtractDocumentIdFromName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        int lastSlash = name.LastIndexOf('/');
        return (lastSlash >= 0 && lastSlash < name.Length - 1) ? name.Substring(lastSlash + 1) : name;
    }

    /// <summary>
    /// Converts a dictionary to a JSON string for Firestore.
    /// </summary>
    private string DictionaryToJson(Dictionary<string, object> dict)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{");
        bool first = true;
        foreach (var kv in dict)
        {
            if (!first) sb.Append(",");
            first = false;
            sb.Append($"\"{kv.Key}\":");
            var value = kv.Value;
            if (value == null)
            {
                sb.Append("null");
            }
            else if (value is string)
            {
                sb.Append($"\"{EscapeJsonString((string)value)}\"");
            }
            else if (value is bool)
            {
                sb.Append((bool)value ? "true" : "false");
            }
            else if (value is IDictionary)
            {
                sb.Append(DictionaryToJson(new Dictionary<string, object>((IDictionary<string, object>)value)));
            }
            else if (value is IList)
            {
                sb.Append("[");
                bool firstInList = true;
                foreach (var elem in (IList)value)
                {
                    if (!firstInList) sb.Append(",");
                    firstInList = false;
                    if (elem is string)
                        sb.Append($"\"{EscapeJsonString((string)elem)}\"");
                    else if (elem is bool)
                        sb.Append((bool)elem ? "true" : "false");
                    else if (elem is IDictionary)
                        sb.Append(DictionaryToJson(new Dictionary<string, object>((IDictionary<string, object>)elem)));
                    else
                        sb.Append(elem.ToString());
                }
                sb.Append("]");
            }
            else
            {
                sb.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        sb.Append("}");
        return sb.ToString();
    }

    /// <summary>
    /// Escapes special characters in a JSON string value.
    /// </summary>
    private string EscapeJsonString(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }
}

/// <summary>
/// A minimal JSON parser for simple JSON structures (for Firestore responses).
/// </summary>
public static class JsonParser
{
    public static object Parse(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        int index = 0;
        return ParseValue(json, ref index);
    }

    private static object ParseValue(string json, ref int index)
    {
        SkipWhitespace(json, ref index);
        if (index >= json.Length) return null;
        char c = json[index];
        if (c == '{') return ParseObject(json, ref index);
        if (c == '[') return ParseArray(json, ref index);
        if (c == '"') return ParseString(json, ref index);
        if (char.IsDigit(c) || c == '-') return ParseNumber(json, ref index);
        if (json.Substring(index).StartsWith("true"))
        {
            index += 4;
            return true;
        }
        if (json.Substring(index).StartsWith("false"))
        {
            index += 5;
            return false;
        }
        if (json.Substring(index).StartsWith("null"))
        {
            index += 4;
            return null;
        }
        return null;
    }

    private static Dictionary<string, object> ParseObject(string json, ref int index)
    {
        var dict = new Dictionary<string, object>();
        index++; // skip '{'
        while (true)
        {
            SkipWhitespace(json, ref index);
            if (index >= json.Length) break;
            if (json[index] == '}')
            {
                index++;
                break;
            }
            string key = ParseString(json, ref index);
            SkipWhitespace(json, ref index);
            if (index < json.Length && json[index] == ':') index++;
            object value = ParseValue(json, ref index);
            dict[key] = value;
            SkipWhitespace(json, ref index);
            if (index < json.Length && json[index] == ',') { index++; continue; }
        }
        return dict;
    }

    private static List<object> ParseArray(string json, ref int index)
    {
        var list = new List<object>();
        index++; // skip '['
        while (true)
        {
            SkipWhitespace(json, ref index);
            if (index >= json.Length) break;
            if (json[index] == ']')
            {
                index++;
                break;
            }
            object value = ParseValue(json, ref index);
            list.Add(value);
            SkipWhitespace(json, ref index);
            if (index < json.Length && json[index] == ',') { index++; continue; }
        }
        return list;
    }

    private static string ParseString(string json, ref int index)
    {
        index++; // skip initial quote
        int start = index;
        while (index < json.Length)
        {
            if (json[index] == '\\')
            {
                index += 2;
                continue;
            }
            if (json[index] == '"') break;
            index++;
        }
        string str = json.Substring(start, index - start);
        index++; // skip closing '"'
        return str.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
    }

    private static object ParseNumber(string json, ref int index)
    {
        int start = index;
        while (index < json.Length && ("-+0123456789.eE".IndexOf(json[index]) != -1))
            index++;
        string number = json.Substring(start, index - start);
        if (number.Contains(".") || number.Contains("e") || number.Contains("E"))
        {
            if (double.TryParse(number, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                return d;
        }
        else
        {
            if (long.TryParse(number, out long l))
                return l;
        }
        return null;
    }

    private static void SkipWhitespace(string json, ref int index)
    {
        while (index < json.Length && char.IsWhiteSpace(json[index]))
            index++;
    }
}
