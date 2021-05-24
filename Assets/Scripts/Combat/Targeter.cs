using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public Targetable GetTarget()
    {
        return target;
    }

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        //nesnesin targetable'ı yok ise
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        // var ise
        target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

  
    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }


}
