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
    private bool _visibleSkytrainEntityCreated = false;
    private Entity _visibleSkytrainEntity;
    private float _visibleSkytrainEntityScale = 1.5f;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Debug.Log("_entityManager.World " + _entityManager.World);
        // Create an entity for the skytrain and add the position component
        _skytrainEntity = _entityManager.CreateEntity(typeof(ConvertedSkytrainProperties));
        // If set to create visible entity, do so
        if (_createVisibleSkytrainEntity && _skytrainVisibleEntityPrefab != null)
        {
            InstantiateVisibleSkytrainEntity();
        }

        // create the loading zone entities
        InstantiateLoadingZoneEntities();
        // set the offset of the loading zone

        // add the component onto the skytrain that references



        /*_entityManager.AddComponent(_entity, new ColliderParentProperties {
            _colliderObject = GetEntity(_colliderObject),
            _colliderPosition = _colliderPosition
        });*/
        Debug.Log(gameObject.name + " entity [" + _skytrainEntity.Index + "]");

        // Initialize the position component with the GameObject's current position
        _entityManager.SetComponentData(_skytrainEntity, new ConvertedSkytrainProperties { Value = (float3)transform.position });
    }

    void Update()
    {
        // Update the entity's position component with the GameObject's current position
        _entityManager.SetComponentData(_skytrainEntity, new ConvertedSkytrainProperties { Value = (float3)transform.position });

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

    private void InstantiateVisibleSkytrainEntity()
    {
        EntityArchetype visibleSkytrainArchetype = _entityManager.CreateArchetype(
                typeof(LocalToWorld)//,
                                    //typeof(RenderBounds),
                                    //typeof(RenderMesh)
            );

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

        _visibleSkytrainEntity = _entityManager.CreateEntity(visibleSkytrainArchetype);

        // Call AddComponents to populate base entity with the components required
        // by Entities Graphics
        RenderMeshUtility.AddComponents(
            _visibleSkytrainEntity,
            _entityManager,
            desc,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        _entityManager.AddComponentData(_visibleSkytrainEntity, new LocalTransform
        {
            Position = this.transform.position,
            Scale = _visibleSkytrainEntityScale,
            Rotation = this.transform.rotation
        });

        _visibleSkytrainEntityCreated = true;
    }
    private void UpdateVisibleSkytrainEntityTransform()
    {
        if ( _visibleSkytrainEntityCreated)
        {
            _entityManager.SetComponentData(_visibleSkytrainEntity, new LocalTransform
            {
                Position = this.transform.position,
                Scale = _visibleSkytrainEntityScale,
                Rotation = this.transform.rotation
            });
        }
        
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

            // Add physics stuff to enable physics
            //PhysicsCollider
            BlobAssetReference<Unity.Physics.Collider> boxColliderBlob = Unity.Physics.BoxCollider.Create(new Unity.Physics.BoxGeometry
            {
                Center = float3.zero,
                BevelRadius = 0.05f,
                Orientation = quaternion.identity,
                Size = new float3(1, 1, 1)
            });
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

            _entityManager.AddBuffer<StatefulCollisionEvent>(loadingZoneEntity);

            /*    .AddComponentData(loadingZoneEntity, new StatefulCollisionEvent
            {
            });*/

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