
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    #region Server


    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;   //sub
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;   //unsub  nesneleri durdurmak için
    }


    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target != null)
        {

            //kovala
            // hedefin konumu ile hedefleyenin konumu arasındaki mesafe, yakalama mesafesinden fazla ise kovala
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            //değilse dur
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if (!agent.hasPath)
        { return; }
        //unitler birbirine çarpadan dursun
        if (agent.remainingDistance > agent.stoppingDistance)
        { return; }

        agent.ResetPath();
    }



    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
   public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }



    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();  // oyun bitince nesneleri durdurur
    }


    #endregion

    //#region Client

    //public override void OnStartAuthority()
    //{
    //    mainCamera = Camera.main;
    //}

    //[ClientCallback]
    //private void Update()
    //{
    //    if (!hasAuthority) { return; }

    //    if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

    //    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

    //    if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

    //    CmdMove(hit.point);
    //}

    //#endregion
}
