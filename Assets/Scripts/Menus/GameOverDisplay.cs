using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent = null;   //birisi kazanana kadar UI active olmamalı... active etmek için referansı kullan 
    [SerializeField] private TMP_Text winnerNameText = null;

    private void Start()
    {

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;

    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    public void LeaveGame()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            //hostingi durdur
            NetworkManager.singleton.StopHost();
        }

        else
        {
            //clientı durdur
            NetworkManager.singleton.StopClient();
        }

    }


    private void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} has won";

        gameOverDisplayParent.SetActive(true);

    }

}

