using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
public class CoinsView : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI coinText;
    public GameRunner gameRunner;
    [Header("Settings")]
    public float coinCountingSpeed = 0.4f;
    public float popTime = 0.4f;
    public float popScale = 0.4f;
    public int popVibrato = 1;
    public float popElasticity = .5f;

    private CoinsViewModel coinViewModel;
    int previousValue;
    private Tweener _counterTween;
    private Tween _popTween;
    private void Start()
    {
        if (gameRunner == null)
        {
            gameRunner = FindAnyObjectByType<GameRunner>();
        }
        if (!gameRunner.gameModel.isNull)
            DisplayGameModel(gameRunner.gameModel);
        gameRunner.gameModel.onChange += DisplayGameModel;
    }
    public void DisplayGameModel(RefEvent<GameModel> gameModel)
    {
        InitViewModel(gameModel.Value.inventoryModel);
    }
    public void InitViewModel(InventoryModel inventoryModel)
    {
        coinViewModel = new CoinsViewModel(inventoryModel);
        coinViewModel.coins.onChange += UpdateCoins;
        previousValue = coinViewModel.coins;
        UpdateCoins(coinViewModel.coins); 
    }
    void UpdateCoins(RefEvent<int> coin)
    {
        if (_counterTween != null && _counterTween.IsActive())
        {
            _counterTween.Kill();
        }

        if (coin.Value != previousValue)
        {
            // Track the last displayed integer to know exactly when a digit ticks up
            int lastDisplayedValue = previousValue;

            _counterTween = DOTween.To(() => previousValue, x =>
            {
                previousValue = x;
                int currentIntValue = x;

                if (currentIntValue != lastDisplayedValue)
                {
                    coinText.text = currentIntValue.ToString();
                    lastDisplayedValue = currentIntValue;

                    TriggerTextPop();
                }
            }, coin.Value, coinCountingSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                coinText.text = coin.Value.ToString();
                previousValue = coin.Value;
            });
        }
        else
        {
            coinText.text = coin.Value.ToString();
        }
    }
    void TriggerTextPop()
    {
        if (_popTween != null && _popTween.IsActive())
        {
            _popTween.Kill();
        }
        coinText.transform.localScale = Vector3.one;

        _popTween = coinText.transform.DOPunchScale(new Vector3(popScale, popScale, 0f), popTime, popVibrato, popElasticity);
    }
    private void OnDestroy()
    {
        if (_counterTween != null && _counterTween.IsActive())
        {
            _counterTween.Kill();
        }
    }
}
