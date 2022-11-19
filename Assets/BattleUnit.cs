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
    public bool IsUpdating => !_navAgent.isStopped || isPrerformingAction;

    [SerializeField] private float _interactionRadius;
    [SerializeField] private GameObject _navMeshObstacle = null;
    [SerializeField] private NavMeshAgent _navAgent = null;

    [SerializeField] private LayerMask _groundCheckLayermask;
    [SerializeField] private GameObject _characterToSpawn;
    [SerializeField] protected GameObject _charactersContainer = null;

    private List<GameObject> _people= new List<GameObject>();

    private bool isPrerformingAction = false;
    private float _radius = 1f;

    public UnityAction OnBattleUnitDestroyed;
    public UnityAction OnBattleUnitMerged;

    private void OnEnable()
    {
        _navAgent ??= GetComponent<NavMeshAgent>();
        _navMeshObstacle ??= GetComponentInChildren<NavMeshObstacle>().gameObject;

        DestroyAllSpawnedPeople();
        UpdatePeopleDistribution();
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
    }

    private void OnDestroy()
    {
        OnBattleUnitDestroyed?.Invoke();
    }

    private void UpdateVerticalPositioning() 
    {
        var pos = Vector3.zero;

        foreach (var human in _people) 
        {
            pos = human.transform.position;
            pos.y = transform.position.y + 1f;
            //Randomoffset for animation
            var randomOffset = Mathf.PerlinNoise(pos.x * 5f, pos.z * 5f) * 0.05f + 0.5f;
            if(Physics.Raycast(pos, Vector3.down, out var hitInfo, 2f, _groundCheckLayermask))
            {
                human.transform.position = hitInfo.point + Vector3.up * randomOffset;
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
                var human = _people[i];
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

        UpdateVerticalPositioning();
    }

    public void AttackBattleUnits(BattleUnit other) 
    {
        if (other != this && other.BattleSide != BattleSide)
        {
            PeopleCount = Mathf.Max(PeopleCount - other.PeopleCount, 0);
            UpdatePeopleDistribution();
        }
    }

    public void MergeBattleUnits(BattleUnit other) 
    {
        if (other != this && other.BattleSide == BattleSide) 
        {
            PeopleCount += other.PeopleCount;
            UpdatePeopleDistribution();
            Destroy(other.gameObject);
            OnBattleUnitMerged?.Invoke();
        }
    }

    public void MoveToPositon(Vector3 position) 
    {
        _navAgent.SetDestination(position);
    }

}

public enum BattleSide 
{
    Poles = 0,
    Crusaders = 1
}
