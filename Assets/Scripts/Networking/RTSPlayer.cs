using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{

    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private Building[] buildings = new Building[0];

    [SyncVar(hook = nameof(ClientHandleGoldUpdated))]
    private int gold = 250;
    public event Action<int> ClientOnGoldUpdated;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;


    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    public static event Action ClientOnInfoUpdated;

    private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }


    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public int GetGold()
    {
        return gold;
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }


    public List<Building> GetMyBuildings()
    {
        return myBuildings;

    }


    //-----------------------------------
    //----------------SERVER-------------
    //-----------------------------------


    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;

        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

        DontDestroyOnLoad(gameObject);

    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;

        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    [Server]
    public void SetGold(int newGold)
    {
        gold = newGold;
    }



    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Command]
    public void CmdStartGame()
    {
        if(!isPartyOwner) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;
        foreach(Building building in buildings)
        {
            if(building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        if(buildingToPlace == null) 
        { return; }

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);


    }



    private void ServerHandleUnitSpawned(Unit unit)
    {
        //unit spawn olunca yada ölünce unit'in id'si ile client'ın id'si aynı mı?
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        //unit spawn olunca yada ölünce unit'in id'si ile client'ın id'si aynı mı?
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }


    private void ServerHandleBuildingSpawned(Building building)
    {
        //building spawn olunca yada ölünce unit'in id'si ile client'ın id'si aynı mı?
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        //building spawn olunca yada ölünce unit'in id'si ile client'ın id'si aynı mı?
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
    }







    #endregion

    //-----------------------------------
    //----------------CLIENT-------------
    //-----------------------------------


    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;

        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;

    }


    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; }

        DontDestroyOnLoad(gameObject);

        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }


    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();
        if (!isClientOnly) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if(!hasAuthority) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;

        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void ClientHandleGoldUpdated(int oldGold, int newGold)
    {
        ClientOnGoldUpdated?.Invoke(newGold);

    }

    public void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if(!hasAuthority) { return; }
        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }
    private void AuthorityHandleUnitSpawned(Unit unit)
    {

        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {

        myUnits.Remove(unit);
    }


    private void AuthorityHandleBuildingSpawned(Building building)
    {

        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {

        myBuildings.Remove(building);
    }



    #endregion
}
