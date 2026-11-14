using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FirstChannel
{
    public sealed class PlayFabLeaderboardClient
    {
        public const string StatisticName = "BestViewers";

        private readonly MonoBehaviour _coroutineRunner;
        private readonly string _titleId;
        private string _sessionTicket;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_sessionTicket);

        public PlayFabLeaderboardClient(MonoBehaviour coroutineRunner, string titleId)
        {
            _coroutineRunner = coroutineRunner;
            _titleId = titleId;
        }

        public void Login(string customId, Action onSuccess, Action<PlayFabError> onError)
        {
            LoginRequest request = new LoginRequest
            {
                TitleId = _titleId,
                CustomId = customId,
                CreateAccount = true
            };

            Post("LoginWithCustomID", JsonUtility.ToJson(request), false, json =>
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(json);
                _sessionTicket = response.data.SessionTicket;
                onSuccess?.Invoke();
            }, onError);
        }

        public void SetDisplayName(string displayName, Action onSuccess, Action<PlayFabError> onError)
        {
            DisplayNameRequest request = new DisplayNameRequest { DisplayName = displayName };
            Post("UpdateUserTitleDisplayName", JsonUtility.ToJson(request), true,
                _ => onSuccess?.Invoke(), onError);
        }

        public void SubmitBestScore(int score, Action onSuccess, Action<PlayFabError> onError)
        {
            StatisticsRequest request = new StatisticsRequest
            {
                Statistics = new[]
                {
                    new StatisticUpdate { StatisticName = StatisticName, Value = score }
                }
            };

            Post("UpdatePlayerStatistics", JsonUtility.ToJson(request), true,
                _ => onSuccess?.Invoke(), onError);
        }

        public void GetTopPlayers(Action<LeaderboardEntry[]> onSuccess, Action<PlayFabError> onError)
        {
            LeaderboardRequest request = new LeaderboardRequest
            {
                StatisticName = StatisticName,
                StartPosition = 0,
                MaxResultsCount = 30
            };

            Post("GetLeaderboard", JsonUtility.ToJson(request), true, json =>
            {
                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);
                onSuccess?.Invoke(response.data.Leaderboard ?? Array.Empty<LeaderboardEntry>());
            }, onError);
        }

        private void Post(
            string method,
            string json,
            bool requiresAuthorization,
            Action<string> onSuccess,
            Action<PlayFabError> onError)
        {
            _coroutineRunner.StartCoroutine(Send(
                method,
                json,
                requiresAuthorization,
                onSuccess,
                onError));
        }

        private IEnumerator Send(
            string method,
            string json,
            bool requiresAuthorization,
            Action<string> onSuccess,
            Action<PlayFabError> onError)
        {
            string url = $"https://{_titleId}.playfabapi.com/Client/{method}";
            byte[] body = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Content-Type", "application/json");

            if (requiresAuthorization)
                request.SetRequestHeader("X-Authorization", _sessionTicket);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
                yield break;
            }

            PlayFabError error = JsonUtility.FromJson<PlayFabError>(request.downloadHandler.text);

            if (error == null || string.IsNullOrEmpty(error.error))
            {
                error = new PlayFabError
                {
                    error = request.error,
                    errorMessage = "Не удалось подключиться к PlayFab."
                };
            }

            onError?.Invoke(error);
        }

        [Serializable]
        private sealed class LoginRequest
        {
            public string TitleId;
            public string CustomId;
            public bool CreateAccount;
        }

        [Serializable]
        private sealed class LoginResponse
        {
            public LoginResult data;
        }

        [Serializable]
        private sealed class LoginResult
        {
            public string SessionTicket;
        }

        [Serializable]
        private sealed class DisplayNameRequest
        {
            public string DisplayName;
        }

        [Serializable]
        private sealed class StatisticsRequest
        {
            public StatisticUpdate[] Statistics;
        }

        [Serializable]
        private sealed class StatisticUpdate
        {
            public string StatisticName;
            public int Value;
        }

        [Serializable]
        private sealed class LeaderboardRequest
        {
            public string StatisticName;
            public int StartPosition;
            public int MaxResultsCount;
        }

        [Serializable]
        private sealed class LeaderboardResponse
        {
            public LeaderboardResult data;
        }

        [Serializable]
        private sealed class LeaderboardResult
        {
            public LeaderboardEntry[] Leaderboard;
        }
    }

    [Serializable]
    public sealed class LeaderboardEntry
    {
        public int Position;
        public string DisplayName;
        public int StatValue;
    }

    [Serializable]
    public sealed class PlayFabError
    {
        public string error;
        public string errorMessage;
    }
}
