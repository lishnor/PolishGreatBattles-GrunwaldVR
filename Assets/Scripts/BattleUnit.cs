using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class BattleUnit : MonoBehaviour
{
    [field: SerializeField] public BattleSide BattleSide { get; private set; }
    [field: SerializeField] public int PeopleCount { get; private set; }
    public bool IsUpdating => isPrerformingAction;

    [SerializeField] private float _interactionRadius;
    [SerializeField] private GameObject _navMeshObstacle = null;
    [SerializeField] private NavMeshAgent _navAgent = null;
    [SerializeField] private float _range = 5f;

    [SerializeField] private LayerMask _otherBattleUnitsLayerMask;
    [SerializeField] private LayerMask _groundCheckLayermask;
    [SerializeField] private GameObject _characterToSpawn;
    [SerializeField] protected GameObject _charactersContainer = null;

    private List<GameObject> _people = new List<GameObject>();

    private bool isPrerformingAction = false;
    private float _radius = 1f;
    private Vector3 _startMovingPosition = Vector3.zero;
    private Collider[] _checkerBufor = new Collider[8];

    public UnityAction OnBattleUnitDestroyed;
    public UnityAction<int> OnUpdateCount;
    public UnityAction OnUpdatePosition;
    public UnityAction OnBattleUnitMerged;

    private void OnEnable()
    {
        _navAgent ??= GetComponent<NavMeshAgent>();
        _navMeshObstacle ??= GetComponentInChildren<NavMeshObstacle>().gameObject;

        DestroyAllSpawnedPeople();
        UpdatePeopleDistribution();
    }

    private void Start()
    {
        UpdatePeopleDistribution();
        Invoke(nameof(UpdateVerticalPositioning), 0.02f);
    }

    private void DestroyAllSpawnedPeople()
    {
        foreach (Transform human in _charactersContainer.transform)
        {
            Destroy(human.gameObject);
        }
    }

    private void Update()
    {
        if (!IsUpdating) return;

        UpdateVerticalPositioning();
        CheckForInteraction();
        CheckDistanceTraveled();
        OnUpdatePosition?.Invoke();
    }

    private void CheckForInteraction()
    {
        var collidersCount = Physics.OverlapSphereNonAlloc(transform.position, 1.1f, _checkerBufor, _otherBattleUnitsLayerMask, QueryTriggerInteraction.Collide);
        
        for (int i = 0; i < collidersCount; i++)
        {
            if (_checkerBufor[i].TryGetComponent<BattleUnit>(out var otherBattleUnit)) 
            {
                if (otherBattleUnit == this) continue;

                if (otherBattleUnit.BattleSide == BattleSide)
                {
                    MergeBattleUnits(otherBattleUnit);
                }
                else 
                {
                    AttackBattleUnits(otherBattleUnit);
                }

                break;
            }
        }
        
    }

    private void CheckDistanceTraveled()
    {
        var traveledDistance = Vector3.Distance(transform.position, _startMovingPosition);
        if (traveledDistance >= _range) 
        {
            StopAgent();
            return;
        }

        traveledDistance = Vector3.Distance(transform.position, _navAgent.destination);
        if (traveledDistance < 0.1f)
        {
            StopAgent();
            return;
        }
    }

    private void StopAgent() 
    {
        isPrerformingAction = false;
        _navAgent.isStopped = true;
        _navAgent.ResetPath();
    }

    private void OnDestroy()
    {
        isPrerformingAction = false;
        OnBattleUnitDestroyed?.Invoke();
    }

    public void ActivateObstacle()
    {
        _navAgent.enabled = false;
        _navMeshObstacle.SetActive(true);
    }

    public void DeactivateObstacle()
    {
        _navMeshObstacle.SetActive(false);
        _navAgent.enabled = true;
    }

    private void UpdateVerticalPositioning() 
    {
        var pos = Vector3.zero;

        foreach (var human in _people) 
        {
            pos = human.transform.position;
            pos.y = transform.position.y + 3f;
            //Randomoffset for animation
            var randomOffset = Mathf.PerlinNoise(pos.x * 5f, pos.z * 5f) * 0.02f + 0.5f;
            if(Physics.Raycast(pos, Vector3.down, out var hitInfo, 4f, _groundCheckLayermask))
            {
                human.transform.position = hitInfo.point + Vector3.up * randomOffset;// randomOffset;
            }
        }
    }

    private void UpdatePeopleDistribution()
    {
        if (PeopleCount <= 0) 
        {
            Destroy(gameObject);
            return;
        }
        
        var spawnedCount = _people.Count;
        var currentCount = PeopleCount;
        var difference = currentCount - spawnedCount;

        //Spawn or despawn
        if (difference < 0)
        {
            difference = Mathf.Abs(difference);
            for (int i = 0; i < difference; i++)
            {
                
                var human = _people[i%_people.Count];
                _people.Remove(human);
                Destroy(human);
            }
        }
        else 
        {
            for (int i = 0; i < difference; i++)
            {
                var spawnedHuman = Instantiate(_characterToSpawn, _charactersContainer.transform);
                _people.Add(spawnedHuman);
            }

            //Distribute Randomly
            foreach (var human in _people)
            {
                var pos = Vector3.zero;
                var random = Random.insideUnitCircle * _radius;

                pos.x = random.x;
                pos.z = random.y;

                human.transform.position = _charactersContainer.transform.TransformPoint(pos);
            }
        }

        OnUpdateCount?.Invoke(currentCount);

        UpdateVerticalPositioning();
    }

    public void AttackBattleUnits(BattleUnit other) 
    {
        if (other != this && other.BattleSide != BattleSide)
        {
            int otherCount = other.PeopleCount;
            int thisCount = PeopleCount;
            PeopleCount = Mathf.Max(thisCount - otherCount, 0);
            other.PeopleCount = Mathf.Max(otherCount - thisCount, 0); ;
            StopAgent();
            other.StopAgent();
            UpdatePeopleDistribution();
            other.UpdatePeopleDistribution();
        }
    }

    public void MergeBattleUnits(BattleUnit other) 
    {
        if (other != this && other.BattleSide == BattleSide) 
        {
            PeopleCount += other.PeopleCount;
            StopAgent();
            UpdatePeopleDistribution();
            Destroy(other.gameObject);
            OnBattleUnitMerged?.Invoke();
        }
    }

    public void MoveToPositon(Vector3 position) 
    {
        isPrerformingAction = true;
        _navAgent.SetDestination(position);
        _startMovingPosition = transform.position;
    }

}

public enum BattleSide 
{
    Poles = 0,
    Crusaders = 1
}
