using Assets.Scripts.Enumerations;
using PlayfulSystems.ProgressBar;
using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Models;
using UnityEngine.SceneManagement;
using Assets.Scripts.Helpers;
using Assets.Scripts.Utilities;
using Assets.Scripts.Models.Pickups;

namespace Assets.Scripts.Managers
{
	public class GuiManager
		: MonoBehaviour
	{
		private const float METER_DELTA = 0.2f;

		[SerializeField]
		private Text coinsText;
		[SerializeField]
		private Text pointsText;
		[SerializeField]
		private Text countDownText;
		[SerializeField]
		private float countdownGrowthDelta;
		[SerializeField]
		private GameObject pauseButton;
		[SerializeField]
		private GameObject pausePanel;
		[SerializeField]
		private Image leftHeart;
		[SerializeField]
		private Image middleHeart;
		[SerializeField]
		private Image rightHeart;
		[SerializeField]
		private GameObject coinDoublerBar;
		[SerializeField]
		private GameObject inhalerBar;
		[SerializeField]
		private GameObject slowmotionBar;
		[SerializeField]
		private GameObject inhalerButton;
		[SerializeField]
		private Color errorColor;
		[SerializeField]
		private Color regularColor;

		[Header("End Screen Panels")]
		[SerializeField]
		private GameObject endScreenPanel;
		[SerializeField]
		private GameObject singleplayerPanel;
		[SerializeField]
		private GameObject multiplayerCreatePanel;
		[SerializeField]
		private GameObject multiplayerChallengePanel;
		[SerializeField]
		private GameObject endScreenExitButton;
		[SerializeField]
		private GameObject factPanel;

		[Header("SP End Screen")]
		[SerializeField]
		private Text spPoints;
		[SerializeField]
		private Text spCoins;
		[SerializeField]
		private Text spTotal;
		[SerializeField]
		private Text spHighscore;
		[SerializeField]
		private Text spHighscoreResult;

		[Header("MP Create End Screen")]
		[SerializeField]
		private Text mpCreatePoints;
		[SerializeField]
		private Text mpCreateCoins;
		[SerializeField]
		private Text mpCreateTotal;
		[SerializeField]
		private Text mpCreateInfo;

		[Header("MP Challenge End Screen")]
		[SerializeField]
		private Color winColor;
		[SerializeField]
		private Color drawColor;
		[SerializeField]
		private Color lossColor;
		[SerializeField]
		private Text mpChallengeMatchResult;
		[SerializeField]
		private Text mpErrorMessage;
		[SerializeField]
		private Text mpChallengePoints;
		[SerializeField]
		private Text mpChallengeCoins;
		[SerializeField]
		private Text mpChallengeTotal;
		[SerializeField]
		private Text mpChallengeFriendTotal;

		[Header("Facts screen")]
		[SerializeField]
		private Text fact;
		[SerializeField]
		private string[] facts;

		private float targetCoinDoublerMeter;
		private float targetInhalerMeter;
		private float targetSlowmotionMeter;

		private ProgressBarPro coinDoubler;
		private ProgressBarPro inhaler;
		private ProgressBarPro slowmotion;

		private RectTransform countdownRect;

		private void Start()
		{
			GameManager.Instance.GuiManager = this;

			coinDoubler = coinDoublerBar.GetComponent<ProgressBarPro>();
			inhaler = inhalerBar.GetComponent<ProgressBarPro>();
			slowmotion = slowmotionBar.GetComponent<ProgressBarPro>();

			inhalerBar.GetComponentInChildren<BarViewSizeImageFill>().SetNumSteps(Inhaler.MAX_AMOUNT);
			countdownRect = countDownText.GetComponent<RectTransform>();
		}

		public void UpdateCoinDoublerMeter(float percentage)
		{
			targetCoinDoublerMeter = percentage;
		}

		public void UpdateInhalerMeter(float percentage)
		{
			targetInhalerMeter = percentage;

			if (Mathf.Approximately(percentage, 1))
			{
				inhalerButton.SetActive(true);
				inhalerButton.GetComponent<Button>().interactable = true;
			}
			else if (Mathf.Approximately(percentage, 0))
			{
				inhalerButton.SetActive(false);
			}
		}

		public void UpdateSlowmotionMeter(float percentage)
		{
			targetSlowmotionMeter = percentage;
		}

		private void FixedUpdate()
		{
			UpdateProgressBar(inhaler, targetInhalerMeter);
			UpdateProgressBar(coinDoubler, targetCoinDoublerMeter);
			UpdateProgressBar(slowmotion, targetSlowmotionMeter);


			if (countdownRect.localScale != Vector3.one)
			{
				countdownRect.localScale = Vector3.MoveTowards(countdownRect.localScale, Vector3.one, countdownGrowthDelta);
			}
		}

		private void UpdateProgressBar(ProgressBarPro progressBar, float target)
		{
			if (!Mathf.Approximately(progressBar.Value, target))
			{
				progressBar.Value = Mathf.MoveTowards(progressBar.Value, target, METER_DELTA);
			}
		}

		public void UpdatePoints(int points)
		{
			pointsText.text = points.ToString().PadLeft(6, '0');
		}

		public void UpdateLives(int lives, bool blink)
		{
			switch (lives)
			{
				case 0:
					FlashImage(leftHeart, blink ? 5 : 0, false);
					middleHeart.enabled = rightHeart.enabled = false;
					break;
				case 1:
					leftHeart.enabled = true;
					FlashImage(middleHeart, blink ? 5 : 0, false);
					rightHeart.enabled = false;
					break;
				case 2:
					leftHeart.enabled = middleHeart.enabled = true;
					FlashImage(rightHeart, blink ? 5 : 0, false);
					break;
				case 3:
					leftHeart.enabled = middleHeart.enabled = rightHeart.enabled = true;
					break;
				default:
					throw new InvalidOperationException("Invalid amount of lives.");
			}
		}

		private void FlashImage(Image image, int amount, bool endState)
		{
			if (amount < 1)
			{
				image.enabled = endState;
				return;
			}

			StartCoroutine(CoroutineHelper.RepeatFor(
						0.25f,
						amount,
						() => image.enabled = !image.enabled,
						() => image.enabled = endState));
		}

		public void UpdateCoins(int coins)
		{
			coinsText.text = String.Format("× {0}", coins.ToString().PadLeft(3, '0'));
		}

		public void DisplayStartSignal(int count)
		{
			if (count < 0)
			{
				countDownText.enabled = false;
				return;
			}

			countDownText.enabled = true;
			countDownText.GetComponent<RectTransform>().localScale = Vector3.zero;

			if (count > 0)
			{
				countDownText.text = String.Format("{0}..", count);
				if (count == 0)
				{
					StartCoroutine(CoroutineHelper.Delay(
						0.25f,
						() => GameManager.Instance.Player.GetComponent<Animator>().SetFloat("Speed", 10)));
				}
			}
			else
			{
				countDownText.text = "GO";
			}
		}

		public void DisplaySingleplayerEndScreen(int points, int coins, HighscoreUpdate highscoreUpdate = null)
		{
			string highscoreText = String.Empty;
			string highscoreResult = String.Empty;
			if (highscoreUpdate != null)
			{
				if (highscoreUpdate.Old == highscoreUpdate.New)
				{
					highscoreText = "Huidige Highscore:";
				}
				else
				{
					highscoreText = "Je hebt een nieuwe Highscore behaald:";
				}

				highscoreResult = highscoreUpdate.New.ToString();
			}

			spHighscore.text = highscoreText;
			spHighscoreResult.text = highscoreResult;
			spPoints.text = points.ToString();
			spCoins.text = coins.ToString();
			spTotal.text = (points + coins).ToString();

			DisplayEndScreen(GameType.Singleplayer);
		}

		public void DisplayMultiplayerCreateEndScreen(int points, int coins, bool success = true)
		{
			if (success)
			{
				mpCreateInfo.color = regularColor;
				mpCreateInfo.text = "Je uitdaging is verstuurd.";
			}
			else
			{
				mpCreateInfo.color = errorColor;
				mpCreateInfo.text = "Uitdaging kon niet verstuurd worden.";
			}

			mpCreatePoints.text = points.ToString();
			mpCreateCoins.text = coins.ToString();
			mpCreateTotal.text = (points + coins).ToString();

			DisplayEndScreen(GameType.MultiplayerCreate);
		}

		public void DisplayMultiplayerChallengeEndScreen(Match match, int points, int coins, bool success = true)
		{
			mpChallengePoints.text = points.ToString();
			mpChallengeCoins.text = coins.ToString();
			mpChallengeTotal.text = (points + coins).ToString();
			mpChallengeFriendTotal.text = match.CreatorScore.ToString();

			if (success)
			{
				mpErrorMessage.text = String.Empty;
				switch (match.Winner)
				{
					case MatchWinner.User:
						mpChallengeMatchResult.color = winColor;
						mpChallengeMatchResult.text = "GEWONNEN";
						SoundManager.Instance.PlaySoundEffect(Sound.Victory);
						break;
					case MatchWinner.Opponent:
						mpChallengeMatchResult.color = lossColor;
						mpChallengeMatchResult.text = "VERLOREN";
						break;
					case MatchWinner.Draw:
						mpChallengeMatchResult.color = drawColor;
						mpChallengeMatchResult.text = "GELIJKSPEL";
						break;
					default:
						throw new InvalidOperationException("Invalid winner.");
				}
			}
			else
			{
				mpErrorMessage.text = "Resultaat kon niet verstuurd worden.";
				mpChallengeMatchResult.text = String.Empty;
			}

			DisplayEndScreen(GameType.MultiplayerChallenge);
		}

		private void DisplayEndScreen(GameType gameType)
		{
			DisableUI();
			endScreenPanel.SetActive(true);

			switch (gameType)
			{
				case GameType.Singleplayer:
					singleplayerPanel.SetActive(true);
					break;
				case GameType.MultiplayerCreate:
					multiplayerCreatePanel.SetActive(true);
					break;
				case GameType.MultiplayerChallenge:
					multiplayerChallengePanel.SetActive(true);
					break;
				default:
					throw new InvalidOperationException("Invalid GameType provided.");
			}
		}

		public void DisableUI()
		{
			pauseButton.SetActive(false);
			inhalerButton.GetComponent<Button>().interactable = false;
		}

		public void PlayAgainButton()
		{
			GameManager.Instance.StartSingleplayerGame();
		}

		public void ActivateInhalerButton()
		{
			if (!GameManager.Instance.Paused)
			{
				inhalerButton.GetComponent<Button>().interactable = false;
				GameManager.Instance.Player.ActivateInhaler(Inhaler.DURATION, Inhaler.SPEED_BONUS);
			}
		}

		public void PauseButton()
		{
			if (GameManager.Instance.Paused)
			{
				pausePanel.SetActive(false);
				pauseButton.SetActive(true);
				GameManager.Instance.Unpause();
			}
			else
			{
				pauseButton.SetActive(false);
				pausePanel.SetActive(true);
				GameManager.Instance.Pause();
			}
		}

		public void ReturnToMainMenuButton()
		{
			if (GameManager.Instance.Paused)
			{
				GameManager.Instance.Unpause();
			}

			SceneManager.LoadScene("MainStartMenu");
		}

		public void DisplayFactScreen()
		{
			endScreenPanel.SetActive(false);
			fact.text = facts.Pick() + " Ga voor meer informatie naar de Serious Request site.";
			factPanel.SetActive(true);
		}

		public void ToSeriousRequestWebsiteButton()
		{
			Application.OpenURL("http://seriousrequest.3fm.nl/nieuws/detail/5351472/doel-3fm-serious-request-2016-longontsteking");
		}
	}
}

