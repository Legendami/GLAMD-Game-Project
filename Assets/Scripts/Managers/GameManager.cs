using Assets.Scripts.Controllers;
using Assets.Scripts.Enumerations;
using Assets.Scripts.Helpers;
using Assets.Scripts.Models;
using Assets.Scripts.Utilities;
using System;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif
using UnityEngine.SceneManagement;
using RNG = System.Random;

namespace Assets.Scripts.Managers
{
	public class GameManager
		: MonoBehaviour
	{
		public const int AD_SHOW_AMOUNT = 5;

		private int adCounter;
		private static readonly RNG rng = new RNG();
		private float unpausedTimeScale;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);

				ShowInstructions = true;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		[SerializeField]
		private LogLevel _logLevel;
		public LogLevel LogLevel
		{
			get
			{
				return _logLevel;
			}
		}

		[SerializeField]
		private bool _localMode;
		public bool LocalMode
		{
			get
			{
				return _localMode;
			}
		}

		[SerializeField]
		private bool _skipLogin;
		public bool SkipLogin
		{
			get
			{
				return _skipLogin;
			}
		}

		[SerializeField]
		private string id;
		[SerializeField]
		private string username;
		public User DummyUser
		{
			get
			{
				return new User
				{
					Id = new Guid(id),
					Username = username
				};
			}
		}

		private User _user;
		public User User
		{
			get
			{
				if (SkipLogin && _user == null)
				{
					return DummyUser;
				}

				return _user;
			}
			set
			{
				_user = value;
			}
		}

		private Game _currentGame;
		public Game CurrentGame
		{
			get
			{
				if (_currentGame != null)
				{
					return _currentGame;
				}

				return new Game();
			}
			private set
			{
				_currentGame = value;
			}
		}

		public static GameManager Instance { get; private set; }

		public PlayerController Player { get; set; }

		public GuiManager GuiManager { get; set; }

		public MenuManager MenuManager { get; set; }

		public bool Paused { get; internal set; }

		public bool ShowInstructions { get; set; }

		public void StartSingleplayerGame()
		{
			SetRandomSeed();
			StartGame(GameType.Singleplayer, null, null, null);
		}

		public void StartMultiplayerGame(Guid opponentId)
		{
			SetRandomSeed();
			StartGame(GameType.MultiplayerCreate, null, new Replay(), opponentId);
		}

		public void StartMultiplayerGame(Match match, Replay replay)
		{
			RandomUtilities.Seed = match.Seed;
			StartGame(GameType.MultiplayerChallenge, match, replay, null);
		}

		private void StartGame(GameType gameType, Match match, Replay replay, Guid? opponentId)
		{
			var game = new Game
			{
				GameType = gameType,
				Match = match,
				Replay = replay,
				OpponentId = opponentId
			};

			if (ShowInstructions)
			{
				StartCoroutine(CoroutineHelper.For(
					2,
					() => 0,
					i => i < 2 && MenuManager != null,
					(ref int i) => i++,
					i => MenuManager.ShowLoadingScreen(i),
					() => StartGame(game)));
			}
			else
			{
				if (MenuManager != null)
				{
					MenuManager.ShowLoadingScreen();
				}

				StartGame(game);
			}
		}

		private void StartGame(Game game)
		{
			CurrentGame = game;

#if UNITY_ADS
			if (adCounter++ % AD_SHOW_AMOUNT == 0)
			{
				Advertisement.Show(new ShowOptions
				{
					resultCallback = result =>
					{
						if (result == ShowResult.Failed)
						{
							Debug.LogWarning("Couldn't play advertisement");
						}

						SceneManager.LoadScene("Main");
					}
				});
			}
			else
			{
				SceneManager.LoadScene("Main");
			}
#else
			SceneManager.LoadScene("Main");
#endif
		}

		public void Logout()
		{
			User = null;
		}

		public void Pause()
		{
			Paused = true;
			unpausedTimeScale = Time.timeScale;
			Time.timeScale = 0;
		}

		public void Unpause()
		{
			Time.timeScale = unpausedTimeScale;
			Paused = false;
		}

		private static void SetRandomSeed()
		{
			RandomUtilities.Seed = rng.Next(Int32.MinValue, Int32.MaxValue);
		}

		public void HandleFinishedMultiplayerCreateGame()
		{
			StartCoroutine(ApiManager.MatchCalls.CreateMatch(
						RandomUtilities.Seed,
						CurrentGame.OpponentId.Value,
						User.Id,
						Player.Points + Player.Coins,
						onSuccess: match =>
						{
							Instance.StartCoroutine(ApiManager.ReplayCalls.CreateReplay(
								match.Id,
								CurrentGame.Replay.ToString(),
								onSuccess: replayId =>
								{
									GuiManager.DisplayMultiplayerCreateEndScreen(Player.Points, Player.Coins);
								},
								onFailure: error =>
								{
									GuiManager.DisplayMultiplayerCreateEndScreen(Player.Points, Player.Coins, false);
								}));
						},
						onFailure: error =>
						{
							GuiManager.DisplayMultiplayerCreateEndScreen(Player.Points, Player.Coins, false);
						}));
		}

		public void HandleFinishedMultiplayerChallengeGame()
		{
			StartCoroutine(ApiManager.MatchCalls.UpdateMatch(
						CurrentGame.Match.Id,
						Player.Points + Player.Coins,
						onSuccess: match =>
						{
							GuiManager.DisplayMultiplayerChallengeEndScreen(match, Player.Points, Player.Coins);
						},
						onFailure: error =>
						{
							GuiManager.DisplayMultiplayerChallengeEndScreen(CurrentGame.Match, Player.Points, Player.Coins, false);
						}));
		}

		public void HandleFinishedSingleplayerGame()
		{
			StartCoroutine(ApiManager.HighscoreCalls.HighscoreAdd(
				User.Username,
				Player.Points + Player.Coins,
				onSuccess: highscoreUpdate => GuiManager.DisplaySingleplayerEndScreen(Player.Points, Player.Coins, highscoreUpdate),
				onFailure: error => GuiManager.DisplaySingleplayerEndScreen(Player.Points, Player.Coins)));
		}
	}
}
