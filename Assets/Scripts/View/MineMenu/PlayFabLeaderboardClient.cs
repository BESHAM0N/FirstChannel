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
        public const string LeaderboardName = "BestViewers";

        private readonly MonoBehaviour _coroutineRunner;
        private readonly string _titleId;
        private string _entityToken;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_entityToken);

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
                CreateAccount = true,
                LoginTitlePlayerAccountEntity = true
            };

            Post("Client/LoginWithCustomID", JsonUtility.ToJson(request), false, json =>
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(json);
                _entityToken = response.data.EntityToken.EntityToken;
                onSuccess?.Invoke();
            }, onError);
        }

        public void SetDisplayName(string displayName, Action onSuccess, Action<PlayFabError> onError)
        {
            DisplayNameRequest request = new DisplayNameRequest { DisplayName = displayName };
            Post("Profile/SetDisplayName", JsonUtility.ToJson(request), true,
                _ => onSuccess?.Invoke(), onError);
        }

        public void SubmitBestScore(int score, Action onSuccess, Action<PlayFabError> onError)
        {
            StatisticsRequest request = new StatisticsRequest
            {
                Statistics = new[]
                {
                    new StatisticUpdate
                    {
                        Name = StatisticName,
                        Scores = new[] { score.ToString() }
                    }
                }
            };

            Post("Statistic/UpdateStatistics", JsonUtility.ToJson(request), true,
                _ => onSuccess?.Invoke(), onError);
        }

        public void GetTopPlayers(Action<LeaderboardEntry[]> onSuccess, Action<PlayFabError> onError)
        {
            LeaderboardRequest request = new LeaderboardRequest
            {
                LeaderboardName = LeaderboardName,
                StartingPosition = 1,
                PageSize = 30
            };

            Post("Leaderboard/GetLeaderboard", JsonUtility.ToJson(request), true, json =>
            {
                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);
                EntityLeaderboardEntry[] rankings =
                    response.data.Rankings ?? Array.Empty<EntityLeaderboardEntry>();
                LeaderboardEntry[] entries = new LeaderboardEntry[rankings.Length];

                for (int i = 0; i < rankings.Length; i++)
                {
                    EntityLeaderboardEntry ranking = rankings[i];
                    int.TryParse(ranking.Scores != null && ranking.Scores.Length > 0
                        ? ranking.Scores[0]
                        : "0", out int score);

                    entries[i] = new LeaderboardEntry
                    {
                        Position = ranking.Rank,
                        DisplayName = ranking.DisplayName,
                        StatValue = score
                    };
                }

                onSuccess?.Invoke(entries);
            }, onError);
        }

        private void Post(
            string path,
            string json,
            bool requiresEntityToken,
            Action<string> onSuccess,
            Action<PlayFabError> onError)
        {
            _coroutineRunner.StartCoroutine(Send(
                path,
                json,
                requiresEntityToken,
                onSuccess,
                onError));
        }

        private IEnumerator Send(
            string path,
            string json,
            bool requiresEntityToken,
            Action<string> onSuccess,
            Action<PlayFabError> onError)
        {
            string url = $"https://{_titleId}.playfabapi.com/{path}";
            byte[] body = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Content-Type", "application/json");

            if (requiresEntityToken)
                request.SetRequestHeader("X-EntityToken", _entityToken);

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
            public bool LoginTitlePlayerAccountEntity;
        }

        [Serializable]
        private sealed class LoginResponse
        {
            public LoginResult data;
        }

        [Serializable]
        private sealed class LoginResult
        {
            public EntityTokenResponse EntityToken;
        }

        [Serializable]
        private sealed class EntityTokenResponse
        {
            public string EntityToken;
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
            public string Name;
            public string[] Scores;
        }

        [Serializable]
        private sealed class LeaderboardRequest
        {
            public string LeaderboardName;
            public int StartingPosition;
            public int PageSize;
        }

        [Serializable]
        private sealed class LeaderboardResponse
        {
            public LeaderboardResult data;
        }

        [Serializable]
        private sealed class LeaderboardResult
        {
            public EntityLeaderboardEntry[] Rankings;
        }

        [Serializable]
        private sealed class EntityLeaderboardEntry
        {
            public int Rank;
            public string DisplayName;
            public string[] Scores;
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
