using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Yuuta.Snake
{
    public class MainGame : MonoBehaviour
    {
        private const string HIGHEST_SCORE_KEY = "Highest_Score";
        
        [SerializeField] private Snake _snake;
        [SerializeField] private Food _food;
        [SerializeField] private TextMeshProUGUI[] _scoreTexts;
        [SerializeField] private GameObject _gameOverPanelGameObject;
        [SerializeField] private TextMeshProUGUI _highestScoreText;
        [SerializeField] private Button _retryButton;

        private Vector2 _sizePerGrid;

        private static readonly (Func<bool> KeyDownChecker, Snake.Direction Direction)[] KEYBOARD_MAP = {
            (() => Input.GetKeyDown(KeyCode.UpArrow) ||
                   Input.GetKeyDown(KeyCode.W), 
                Snake.Direction.Up),
            (() => Input.GetKeyDown(KeyCode.DownArrow) ||
                   Input.GetKeyDown(KeyCode.S), 
                Snake.Direction.Down),
            (() => Input.GetKeyDown(KeyCode.LeftArrow) ||
                   Input.GetKeyDown(KeyCode.A), 
                Snake.Direction.Left),
            (() => Input.GetKeyDown(KeyCode.RightArrow) ||
                   Input.GetKeyDown(KeyCode.D), 
                Snake.Direction.Right),
        };
        
        private async void Start()
        {
            using (_RegisterEvents())
            {
                while (true)
                {
                    await _RunReadyStage();
                    await _RunGameStage();
                    await _RunResultStage();
                }
            }
        }

        private IDisposable _RegisterEvents()
        {
            var compositeDisposable = new CompositeDisposable();
            foreach (var valueTuple in KEYBOARD_MAP)
            {
                compositeDisposable.Add(Observable.EveryUpdate()
                    .Where(_ => valueTuple.KeyDownChecker())
                    .Subscribe(_ => _snake.ChangeDirection(valueTuple.Direction))
                    .AddTo(this));
            }
            compositeDisposable.Add(Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Space))
                .Subscribe( _ => UniTask.Void(async () => 
                {
                    if (_snake.IsRushed)
                        return;
                    
                    _snake.ChangeToRushMode();
                    await UniTask.Yield();
                    _snake.ChangeToNormalMode();
                }))
                .AddTo(this));
            
            return compositeDisposable;
        }
        
        private async UniTask _RunReadyStage()
        {
            _gameOverPanelGameObject.SetActive(false);
            _snake.Initialize();
            _food.RandomPut();

            await UniTask.CompletedTask;
        }

        private async UniTask _RunGameStage()
        {
            while (!_snake.IsDead)
            {
                _snake.UpdateOneStep();
                foreach (var scoreText in _scoreTexts)
                {
                    scoreText.text = $"Score: {_snake.Length}";
                }
                await UniTask.Yield();
            }
        }
        
        private async UniTask _RunResultStage()
        {
            int maxScore = Mathf.Max(
                _snake.Length,
                PlayerPrefs.GetInt(HIGHEST_SCORE_KEY, 0));
            PlayerPrefs.SetInt(HIGHEST_SCORE_KEY, maxScore);
            PlayerPrefs.Save();
            
            bool isRetry = false;
            _gameOverPanelGameObject.SetActive(true);
            _highestScoreText.text = $"Highest Score: {maxScore}";

            using (_retryButton.OnClickAsObservable().Subscribe(_ => isRetry = true))
            {
                await UniTask.WaitWhile(() => !isRetry);
            }
        }
    }
}