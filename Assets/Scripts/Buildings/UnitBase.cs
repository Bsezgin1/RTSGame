﻿using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{

    [SerializeField] private Health health = null;

    public static event Action<int> ServerOnPlayerDie;  //ölen player'ın id'si ile aynı id'ye sahip tüm nesneler ölsün..
    public static event Action <UnitBase> ServerOnBaseSpawned;
    public static event Action <UnitBase> ServerOnBaseDespawned;

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBaseDespawned?.Invoke(this);
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }



    #endregion









    #region Client





    #endregion
}
