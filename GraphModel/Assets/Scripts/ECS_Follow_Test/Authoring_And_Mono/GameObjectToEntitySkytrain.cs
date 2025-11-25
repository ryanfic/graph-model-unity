using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Physics.Stateful;

public class GameObjectToEntitySkytrain : MonoBehaviour
{
    public GameObject _loadingZonePrefab;
    public float _loadingZoneScale = 1.0f;
    public List<Vector3> _loadingZoneOffsets;
    public bool _createVisibleSkytrainEntity = true;
    public GameObject _skytrainVisibleEntityPrefab;

    private EntityManager _entityManager;
    private Entity _skytrainEntity;
    private List<Entity> _loadingZoneEntities = new List<Entity>();
    private Entity _visibleSkytrainEntity;
    private float _visibleSkytrainEntityScale = 1.5f;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Debug.Log("_entityManager.World " + _entityManager.World);

        InstantiateSkytrainEntity();

        // create the loading zone entities
        InstantiateLoadingZoneEntities();

        Debug.Log(gameObject.name + " entity [" + _skytrainEntity.Index + "]");

    }

    void Update()
    {
        UpdateVisibleSkytrainEntityTransform();

        UpdateLoadingZonesTransforms();
    }

    void OnDestroy()
    {
        // Destroy the entity when the GameObject is destroyed
        if (_entityManager.Exists(_skytrainEntity))
        {
            
            _entityManager.DestroyEntity(_skytrainEntity);
        }
    }

    private void InstantiateSkytrainEntity()
    {
        // Create an entity for the skytrain and add the position component
        _skytrainEntity = _entityManager.CreateEntity(typeof(SkytrainProperties));
        _entityManager.SetComponentData(_skytrainEntity, new SkytrainProperties {
            SkytrainName = "SkytrainCoolio",
            MaxCapacity = 500,
            CurrentCapacity = 0
        });
        _entityManager.AddComponentData(_skytrainEntity, new LocalTransform
        {
            Position = this.transform.position,
            Scale = _visibleSkytrainEntityScale,
            Rotation = this.transform.rotation
        });
        _entityManager.AddComponent<LocalToWorld>(_skytrainEntity);

        // If set to create visible entity, add relevant components to make it visible
        if (_createVisibleSkytrainEntity && _skytrainVisibleEntityPrefab != null)
        {
            InstantiateVisibleSkytrainEntity();
        }
        // If was supposed to make a visible entity, but did not set the prefab
        else if (_createVisibleSkytrainEntity) 
        {
            Debug.LogError("Was supposed to create a visible skytrain, but the skytrain lacked a necessary prefab");
        }
    }

    private void InstantiateVisibleSkytrainEntity()
    {
        // Create a RenderMeshDescription using the convenience constructor
        // with named parameters.
        var desc = new RenderMeshDescription(
            shadowCastingMode: ShadowCastingMode.Off,
            receiveShadows: false);

        // Create an array of mesh and material required for runtime rendering.
        //From variables
        //var renderMeshArray = new RenderMeshArray(new Material[] { Material }, new Mesh[] { Mesh });
        //From prefab
        var renderMeshArray = new RenderMeshArray(new Material[] {
           _skytrainVisibleEntityPrefab.GetComponent<Renderer>().sharedMaterial
        }, new Mesh[] {
            _skytrainVisibleEntityPrefab.GetComponent<MeshFilter>().sharedMesh
        });

        // Call AddComponents to populate base entity with the components required
        // by Entities Graphics
        RenderMeshUtility.AddComponents(
            _skytrainEntity,
            _entityManager,
            desc,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        
    }
    private void UpdateVisibleSkytrainEntityTransform()
    {
        _entityManager.SetComponentData(_skytrainEntity, new LocalTransform
        {
            Position = this.transform.position,
            Scale = _visibleSkytrainEntityScale,
            Rotation = this.transform.rotation
        });
        
    }

    private void InstantiateLoadingZoneEntities()
    {
        // Create a RenderMeshDescription using the convenience constructor
        // with named parameters.
        var desc = new RenderMeshDescription(
            shadowCastingMode: ShadowCastingMode.Off,
            receiveShadows: false);

        // Create an array of mesh and material required for runtime rendering.
        //From prefab
        var renderMeshArray = new RenderMeshArray(new Material[] {
            _loadingZonePrefab.GetComponent<Renderer>().sharedMaterial
        }, new Mesh[] {
            _loadingZonePrefab.GetComponent<MeshFilter>().sharedMesh
        });

        List<string> entityMessages = new List<string>();
        entityMessages.Add("One message");
        entityMessages.Add("Two message");
        entityMessages.Add("Three message");

        for (int i = 0; i < _loadingZoneOffsets.Count; i++)
        {
            Vector3 offset = _loadingZoneOffsets[i];

            Entity loadingZoneEntity = _entityManager.CreateEntity(typeof(LocalToWorld));

            // Call AddComponents to populate base entity with the components required
            // by Entities Graphics
            RenderMeshUtility.AddComponents(
                loadingZoneEntity,
                _entityManager,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
            _entityManager.AddComponentData(loadingZoneEntity, new LocalTransform
            {
                Position = this.transform.position + this.transform.rotation * offset,
                Scale = _loadingZoneScale,
                Rotation = this.transform.rotation
            });

            _entityManager.AddComponentData(loadingZoneEntity, new LoadingZoneComponent { 
                SkytrainEntity = _skytrainEntity
            });

            // Add physics stuff to enable physics
            //PhysicsFilter
            Unity.Physics.CollisionFilter colFilter = Unity.Physics.CollisionFilter.Default;
            // Would change what the trigger can collide with here
            //Material (for physics, not colour)
            Unity.Physics.Material colMaterial = Unity.Physics.Material.Default;
            colMaterial.CollisionResponse = Unity.Physics.CollisionResponsePolicy.RaiseTriggerEvents; // This makes it a trigger
            //PhysicsCollider
            BlobAssetReference <Unity.Physics.Collider> boxColliderBlob = Unity.Physics.BoxCollider.Create(new Unity.Physics.BoxGeometry
                {
                    Center = float3.zero,
                    BevelRadius = 0.05f,
                    Orientation = quaternion.identity,
                    Size = new float3(1, 1, 1)
                },
                colFilter,
                colMaterial
            );
            _entityManager.AddComponentData(loadingZoneEntity, new Unity.Physics.PhysicsCollider { Value = boxColliderBlob });
            //PhysicsVelocity
            _entityManager.AddComponentData(loadingZoneEntity, new Unity.Physics.PhysicsVelocity
            { });
            //PhysicsMass
            //PhysicsWorldIndex
            _entityManager.AddSharedComponentManaged(loadingZoneEntity, new Unity.Physics.PhysicsWorldIndex
            {
                Value = 0
            });

            // Add a message to identify the loading zone
            _entityManager.AddComponentData(loadingZoneEntity, new MessageComponent
            {
                Message = entityMessages[i % entityMessages.Count]
            });

            //_entityManager.AddBuffer<StatefulCollisionEvent>(loadingZoneEntity); // For Collisions
            _entityManager.AddBuffer<StatefulTriggerEvent>(loadingZoneEntity); // For Triggers

            _loadingZoneEntities.Add(loadingZoneEntity);
        }
    }

    private void UpdateLoadingZonesTransforms()
    {
        for (int i = 0; i < _loadingZoneOffsets.Count; i++)
        {
            var offset = _loadingZoneOffsets[i];

            _entityManager.SetComponentData(_loadingZoneEntities[i], new LocalTransform
            {
                Position = this.transform.position + this.transform.rotation * offset,
                Scale = _loadingZoneScale,
                Rotation = this.transform.rotation
            });
        }
            
    }
}