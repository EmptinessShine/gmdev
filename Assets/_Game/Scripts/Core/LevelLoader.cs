using Unity.AI.Navigation;
using UnityEngine;
using TD.Data;
using TD.Enemies;
using TD.Save;

namespace TD.Core
{
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelData fallbackLevel;
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private Transform dynamicWallsParent;
        [SerializeField] private Transform slotsParent;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform basePoint;
        [SerializeField] private MeshRenderer groundRenderer;
        [SerializeField] private BuildSlot buildSlotPrefab;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private WaveSpawner waveSpawner;

        public LevelData ActiveLevel { get; private set; }
        public Vector3 BasePosition => basePoint != null ? basePoint.position : Vector3.zero;

        public LevelData LoadActiveLevel()
        {
            ActiveLevel = SaveManager.Instance != null && SaveManager.Instance.GetSelectedLevel() != null
                ? SaveManager.Instance.GetSelectedLevel()
                : fallbackLevel;

            if (ActiveLevel == null)
            {
                return null;
            }

            if (groundRenderer != null)
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                groundRenderer.GetPropertyBlock(mpb);
                mpb.SetColor("_BaseColor", ActiveLevel.groundColor);
                groundRenderer.SetPropertyBlock(mpb);
            }

            ClearChildren(dynamicWallsParent);
            ClearChildren(slotsParent);

            int wallLayer = LayerMask.NameToLayer("Wall");
            if (ActiveLevel.interiorWalls != null && dynamicWallsParent != null)
            {
                for (int i = 0; i < ActiveLevel.interiorWalls.Length; i++)
                {
                    WallBlock wall = ActiveLevel.interiorWalls[i];
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = $"Wall_Dynamic_{i + 1}";
                    go.layer = wallLayer >= 0 ? wallLayer : 0;
                    go.transform.SetParent(dynamicWallsParent, false);
                    go.transform.localPosition = wall.position;
                    go.transform.localScale = wall.scale;
                    if (wallMaterial != null)
                    {
                        go.GetComponent<MeshRenderer>().sharedMaterial = wallMaterial;
                    }
                }
            }

            if (spawnPoint != null) spawnPoint.position = ActiveLevel.spawnPosition;
            if (basePoint != null) basePoint.position = ActiveLevel.basePosition;

            if (ActiveLevel.slotPositions != null && slotsParent != null && buildSlotPrefab != null)
            {
                for (int i = 0; i < ActiveLevel.slotPositions.Length; i++)
                {
                    BuildSlot slot = Instantiate(buildSlotPrefab, ActiveLevel.slotPositions[i], Quaternion.identity, slotsParent);
                    slot.name = $"BuildSlot_{i + 1}";
                }
            }

            if (navMeshSurface != null)
            {
                Physics.SyncTransforms();
                navMeshSurface.BuildNavMesh();
            }

            if (waveSpawner != null && ActiveLevel.waveSet != null)
            {
                waveSpawner.Configure(ActiveLevel.waveSet);
            }

            return ActiveLevel;
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    child.SetActive(false);
                    child.transform.SetParent(null, false);
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }
    }
}
