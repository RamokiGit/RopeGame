using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] NodeManager _nodeManager;
    [SerializeField] GameObject _completeLabel;
    [SerializeField] Text _completeLevelText;
    private void Awake()
    {
        _nodeManager.OnTypeComplete += CompleteLevel;
    }

   
    private void Start()
    {
        GameStatus.CurrentGameState = GameState.Game;
        _nodeManager.SetupLevel();
    }

    private void OnDisable()
    {
        _nodeManager.OnTypeComplete -= CompleteLevel;
    }
    
    private void CompleteLevel(TypeComplete type)
    {
        switch (type)
        {
            case TypeComplete.Skip:
                ShowCompleteLevelLabel();
                _completeLevelText.text = "Level Complete";
                DOVirtual.DelayedCall(1f,ShowReadyRope);
                break;
            case TypeComplete.SelfComplete:
                ShowCompleteLevelLabel();
                _completeLevelText.text = "Level Win";
                GameVariable.Score += 350;
                DOVirtual.DelayedCall(2.5f,()=> SceneManager.LoadScene((int)Scene.Map));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void ShowReadyRope()
    {
        _completeLabel.SetActive(false);
        DOVirtual.DelayedCall(3f,()=> SceneManager.LoadScene((int)Scene.Map));
    }

    private void ShowCompleteLevelLabel()
    {
        _completeLabel.SetActive(true);
        _completeLabel.transform.DOScale(Vector3.one, 0.7f).From(Vector3.zero).SetEase(Ease.OutBounce);
    }
}

public enum TypeComplete
{
    Skip,
    SelfComplete
}