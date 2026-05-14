using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGuidanceSigns : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform objectiveTarget;
    [SerializeField] private bool createSigns = true;
    [SerializeField] private bool createObjectiveMarker = true;
    [SerializeField] private float signHeight = 1.55f;

    private readonly List<Transform> guideSigns = new List<Transform>();
    private Transform objectiveMarker;
    private Camera playerCamera;
    private Material signMaterial;
    private Material markerMaterial;
    private Health targetHealth;

    private void Start()
    {
        ResolveReferences();

        signMaterial = CreateMaterial("GuideSign_Dark", new Color(0.05f, 0.07f, 0.08f, 0.95f));
        markerMaterial = CreateMaterial("GuideTarget_Amber", new Color(1f, 0.58f, 0.12f, 1f));

        if (createSigns)
        {
            CreateSignsForCurrentLevel();
        }

        if (createObjectiveMarker)
        {
            CreateObjectiveMarker();
        }
    }

    private void LateUpdate()
    {
        Transform viewer = GetViewerTransform();
        if (viewer == null)
        {
            return;
        }

        foreach (Transform sign in guideSigns)
        {
            FaceViewer(sign, viewer);
        }

        if (objectiveMarker != null && objectiveTarget != null)
        {
            if (targetHealth != null && targetHealth.IsDead)
            {
                objectiveMarker.gameObject.SetActive(false);
                return;
            }

            objectiveMarker.position = objectiveTarget.position + Vector3.up * 2.1f
                + Vector3.up * Mathf.Sin(Time.time * 3f) * 0.15f;
            objectiveMarker.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
        }
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("player") ?? GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
        }

        if (player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (objectiveTarget != null)
        {
            targetHealth = objectiveTarget.GetComponent<Health>();
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level3"))
        {
            HostageRescue hostage = FindFirstObjectByType<HostageRescue>();
            objectiveTarget = hostage != null ? hostage.transform : FindNamedTransform("Sitting Idle", "Hostage");
        }
        else if (sceneName.Contains("Level2"))
        {
            objectiveTarget = FindNamedTransform("Bot-Body1", "Bot-Body1 (1)", "Bot-Body1 (2)") ?? FindClosestEnemy();
        }

        if (objectiveTarget != null)
        {
            targetHealth = objectiveTarget.GetComponent<Health>();
        }
    }

    private void CreateSignsForCurrentLevel()
    {
        if (player == null)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level3"))
        {
            CreateLevel3Signs();
        }
        else
        {
            CreateLevel2Signs();
        }
    }

    private void CreateLevel2Signs()
    {
        Vector3 forward;
        Vector3 right;
        GetPlayerAxes(out forward, out right);

        Vector3 playerPosition = player.position;

        CreateSign(
            "GuideSign_Level2Mission",
            playerPosition + forward * 5.5f + right * -2.4f + Vector3.up * signHeight,
            "MISSION 2\nFactory Sweep\nClear every hostile.\nTimer is running.",
            2.9f,
            1.35f);

        CreateSign(
            "GuideSign_Level2Combat",
            playerPosition + forward * 11f + right * 2.6f + Vector3.up * signHeight,
            "COMBAT TIP\nUse cover.\nWatch ammo count.\nReload before pushing.",
            2.9f,
            1.35f);

        if (objectiveTarget != null)
        {
            Vector3 targetDirection = GetDirectionFromTargetToPlayer(forward);
            CreateSign(
                "GuideSign_Level2Objective",
                objectiveTarget.position + targetDirection * 3f + Vector3.up * signHeight,
                "HOSTILE AREA\nEliminate all Bots\nto unlock Level 3.",
                2.8f,
                1.2f);
        }
    }

    private void CreateLevel3Signs()
    {
        Vector3 forward;
        Vector3 right;
        GetPlayerAxes(out forward, out right);

        Vector3 playerPosition = player.position;

        CreateSign(
            "GuideSign_Level3Mission",
            playerPosition + forward * 5.5f + right * -2.4f + Vector3.up * signHeight,
            "MISSION 3\nRescue Operation\nFind the hostage.\nStay alert.",
            2.9f,
            1.35f);

        CreateSign(
            "GuideSign_Level3Route",
            playerPosition + forward * 12f + right * 2.6f + Vector3.up * signHeight,
            "HOSTAGE ROUTE\nFollow the marker.\nMove close to interact.\nProtect your approach.",
            3f,
            1.4f);

        if (objectiveTarget != null)
        {
            Vector3 targetDirection = GetDirectionFromTargetToPlayer(forward);
            CreateSign(
                "GuideSign_Level3Rescue",
                objectiveTarget.position + targetDirection * 2.8f + Vector3.up * signHeight,
                "RESCUE POINT\nStand close.\nHold E until bar fills.\nMission ends after rescue.",
                3f,
                1.4f);
        }
    }

    private void CreateSign(string signName, Vector3 position, string text, float width, float height)
    {
        GameObject signRoot = new GameObject(signName);
        signRoot.transform.SetParent(transform, true);
        signRoot.transform.position = position;
        guideSigns.Add(signRoot.transform);

        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "Board";
        board.transform.SetParent(signRoot.transform, false);
        board.transform.localPosition = Vector3.zero;
        board.transform.localRotation = Quaternion.identity;
        board.transform.localScale = new Vector3(width, height, 0.08f);

        Collider boardCollider = board.GetComponent<Collider>();
        if (boardCollider != null)
        {
            Destroy(boardCollider);
        }

        Renderer boardRenderer = board.GetComponent<Renderer>();
        if (boardRenderer != null && signMaterial != null)
        {
            boardRenderer.material = signMaterial;
        }

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(signRoot.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0f, 0.07f);
        textObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        textObject.transform.localScale = Vector3.one;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.07f;
        textMesh.lineSpacing = 0.85f;
        textMesh.color = Color.white;

        FaceViewer(signRoot.transform, GetViewerTransform());
    }

    private void CreateObjectiveMarker()
    {
        if (objectiveTarget == null)
        {
            return;
        }

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "Guide_ObjectiveMarker";
        marker.transform.SetParent(transform, true);
        marker.transform.position = objectiveTarget.position + Vector3.up * 2.1f;
        marker.transform.localScale = new Vector3(0.45f, 0.08f, 0.45f);

        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
        {
            Destroy(markerCollider);
        }

        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null && markerMaterial != null)
        {
            markerRenderer.material = markerMaterial;
        }

        objectiveMarker = marker.transform;
    }

    private void GetPlayerAxes(out Vector3 forward, out Vector3 right)
    {
        forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.forward;
        }

        right = Vector3.Cross(Vector3.up, forward).normalized;
    }

    private Vector3 GetDirectionFromTargetToPlayer(Vector3 fallbackForward)
    {
        Vector3 direction = Vector3.ProjectOnPlane(player.position - objectiveTarget.position, Vector3.up).normalized;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = -fallbackForward;
        }

        return direction;
    }

    private Transform FindClosestEnemy()
    {
        if (player == null)
        {
            return null;
        }

        Health[] healths = FindObjectsByType<Health>(FindObjectsSortMode.None);
        float bestDistance = float.MaxValue;
        Transform bestTarget = null;

        foreach (Health health in healths)
        {
            if (health == null || health.transform == player)
            {
                continue;
            }

            float distance = Vector3.SqrMagnitude(health.transform.position - player.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = health.transform;
            }
        }

        return bestTarget;
    }

    private static Transform FindNamedTransform(params string[] names)
    {
        foreach (string targetName in names)
        {
            GameObject found = GameObject.Find(targetName);
            if (found != null)
            {
                return found.transform;
            }
        }

        return null;
    }

    private Transform GetViewerTransform()
    {
        if (playerCamera != null)
        {
            return playerCamera.transform;
        }

        return player;
    }

    private static void FaceViewer(Transform sign, Transform viewer)
    {
        if (sign == null || viewer == null)
        {
            return;
        }

        Vector3 direction = viewer.position - sign.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            sign.rotation = Quaternion.LookRotation(direction);
        }
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Unlit/Color") ??
            Shader.Find("Sprites/Default") ??
            Shader.Find("Standard");

        if (shader == null)
        {
            Debug.LogWarning("Level guidance signs could not find a shader for runtime materials.");
            return null;
        }

        Material material = new Material(shader)
        {
            name = materialName,
            color = color
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }
}
