using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : NetworkBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        if(!Mouse.current.rightButton.wasPressedThisFrame)              //right click yapınca raycast çalışır 
        { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))    //yoksa boş dön varsa hareket et
        { return; }

        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))    //Targetable bir nesne ise hedef al
        {
            if (target.hasAuthority)// target senin nesnen ise yürü
            {
                TryMove(hit.point);
                return;
            }

            TryTarget(target);
            return;
        }



        TryMove(hit.point);
    }

    private void TryMove(Vector3 point)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }

    private void TryTarget(Targetable target)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }


    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;    //game over olunca raycast, hareket, atak sonlanır
    }

}
