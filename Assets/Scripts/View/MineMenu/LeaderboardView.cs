using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class LeaderboardView : MonoBehaviour
    {
        private const string PlayerIdPrefsKey = "playfab_player_id";
        private const string PlayerNamePrefsKey = "playfab_player_name";

        [Header("PlayFab")] [SerializeField] private string _titleId = "1E230E";

        [Header("Nickname")]
        [SerializeField] private GameObject _inputNamePanel;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button _applyButton;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private Button _playButton;

        [Header("Leaderboard")]
        [SerializeField] private TMP_Text _myPointsText;
        [SerializeField] private Transform _rowsParent;
        [SerializeField] private LeaderboardRowView _rowTemplate;

        private readonly List<LeaderboardRowView> _rows = new List<LeaderboardRowView>();
        private PlayFabLeaderboardClient _client;
        private bool _isSubmittingName;

        private void Awake()
        {
            _titleId = (_titleId ?? string.Empty).Trim();
            _client = new PlayFabLeaderboardClient(this, _titleId);
            _rowTemplate.gameObject.SetActive(false);
            HideError();

            int bestScore = PlayerPrefs.GetInt(GameCycle.BestViewersPrefsKey, 0);
            _myPointsText.SetText("{0}", bestScore);

            string playerName = PlayerPrefs.GetString(PlayerNamePrefsKey, string.Empty);
            bool hasPlayerName = !string.IsNullOrWhiteSpace(playerName);

            _nameInput.characterLimit = 25;
            _nameInput.text = playerName;
            _inputNamePanel.SetActive(!hasPlayerName);
            _playButton.interactable = hasPlayerName;

            if (string.IsNullOrWhiteSpace(_titleId))
            {
                ShowError("Укажите PlayFab Title ID в LeaderboardView.");
                _applyButton.interactable = false;
                return;
            }

            _applyButton.interactable = false;
            Login(hasPlayerName);
        }

        private void OnEnable()
        {
            _applyButton.onClick.AddListener(ApplyNickname);
        }

        private void OnDisable()
        {
            _applyButton.onClick.RemoveListener(ApplyNickname);
        }

        private void Login(bool loadLeaderboardAfterLogin)
        {
            string playerId = GetOrCreatePlayerId();
            _client.Login(
                playerId,
                () =>
                {
                    _applyButton.interactable = true;

                    if (loadLeaderboardAfterLogin)
                        SyncScoreAndLoadLeaderboard();
                },
                error =>
                {
                    _applyButton.interactable = true;
                    ShowError(error.errorMessage);
                });
        }

        private void ApplyNickname()
        {
            if (_isSubmittingName)
                return;

            string playerName = _nameInput.text.Trim();

            if (playerName.Length < 3 || playerName.Length > 25)
            {
                ShowError("Ник должен содержать от 3 до 25 символов.");
                return;
            }

            HideError();
            _isSubmittingName = true;
            _applyButton.interactable = false;

            if (_client.IsLoggedIn)
                SaveNickname(playerName);
            else
                _client.Login(GetOrCreatePlayerId(), () => SaveNickname(playerName), OnNicknameError);
        }

        private void SaveNickname(string playerName)
        {
            _client.SetDisplayName(
                playerName,
                () =>
                {
                    PlayerPrefs.SetString(PlayerNamePrefsKey, playerName);
                    PlayerPrefs.Save();

                    _isSubmittingName = false;
                    _applyButton.interactable = true;
                    _inputNamePanel.SetActive(false);
                    _playButton.interactable = true;
                    SyncScoreAndLoadLeaderboard();
                },
                OnNicknameError);
        }

        private void OnNicknameError(PlayFabError error)
        {
            _isSubmittingName = false;
            _applyButton.interactable = true;

            switch (error.error)
            {
                case "NameNotAvailable":
                    ShowError("Этот ник уже занят.");
                    break;
                case "ProfaneDisplayName":
                    ShowError("Этот ник нельзя использовать.");
                    break;
                default:
                    ShowError(error.errorMessage);
                    break;
            }
        }

        private void SyncScoreAndLoadLeaderboard()
        {
            int bestScore = PlayerPrefs.GetInt(GameCycle.BestViewersPrefsKey, 0);
            _client.SubmitBestScore(bestScore, LoadLeaderboard, _ => LoadLeaderboard());
        }

        private void LoadLeaderboard()
        {
            _client.GetTopPlayers(ShowLeaderboard, error => Debug.LogWarning(error.errorMessage));
        }

        private void ShowLeaderboard(LeaderboardEntry[] entries)
        {
            for (int i = 0; i < _rows.Count; i++)
                Destroy(_rows[i].gameObject);

            _rows.Clear();

            for (int i = 0; i < entries.Length; i++)
            {
                LeaderboardRowView row = Instantiate(_rowTemplate, _rowsParent);
                row.SetData(entries[i]);
                row.gameObject.SetActive(true);
                _rows.Add(row);
            }

            RectTransform rowsRect = (RectTransform)_rowsParent;
            rowsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, entries.Length * 80f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rowsRect);
        }

        private string GetOrCreatePlayerId()
        {
            string playerId = PlayerPrefs.GetString(PlayerIdPrefsKey, string.Empty);

            if (!string.IsNullOrEmpty(playerId))
                return playerId;

            playerId = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(PlayerIdPrefsKey, playerId);
            PlayerPrefs.Save();
            return playerId;
        }

        private void ShowError(string message)
        {
            _errorText.SetText(string.IsNullOrEmpty(message) ? "Произошла ошибка." : message);
            _errorText.gameObject.SetActive(true);
        }

        private void HideError()
        {
            _errorText.gameObject.SetActive(false);
        }
    }
}
