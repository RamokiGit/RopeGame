using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
   [SerializeField] Button _buttonGame;
   [SerializeField] Text _scoreValue;

   private void Awake()
   {
      _buttonGame.onClick.AddListener(SwitchSceneToGame);
      _scoreValue.text = GameVariable.Score.ToString();
   }

   private void SwitchSceneToGame()
   {
      SceneManager.LoadScene((int)Scene.Game);
   }

   void OnDisable()
   {
      _buttonGame.onClick.RemoveAllListeners();
   }
   
}
