using System.Collections.Generic;
using UnityEngine;

public class Level1TutorialGuide : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Health firstBot;
    [SerializeField] private bool createSigns = true;
    [SerializeField] private bool createTargetMarker = true;
    [SerializeField] private float signHeight = 1.55f;

    private readonly List<Transform> tutorialSigns = new List<Transform>();
    private Transform targetMarker;
    private Vector3 targetMarkerBasePosition;
    private Camera playerCamera;
    private Material signMaterial;
    private Material markerMaterial;

    private void Start()
    {
        ResolveReferences();

        signMaterial = CreateMaterial("TutorialSign_Dark", new Color(0.05f, 0.07f, 0.08f, 0.95f));
        markerMaterial = CreateMaterial("TutorialTarget_Amber", new Color(1f, 0.58f, 0.12f, 1f));

        if (createSigns)
        {
            CreateTutorialSigns();
        }

        if (createTargetMarker)
        {
            CreateTargetMarker();
        }
    }

    private void LateUpdate()
    {
        Transform viewer = GetViewerTransform();
        if (viewer == null)
        {
            return;
        }

        foreach (Transform sign in tutorialSigns)
        {
            FaceViewer(sign, viewer);
        }

        if (targetMarker != null && firstBot != null && !firstBot.IsDead)
        {
            targetMarkerBasePosition = firstBot.transform.position + Vector3.up * 2.1f;
            targetMarker.position = targetMarkerBasePosition + Vector3.up * Mathf.Sin(Time.time * 3f) * 0.15f;
            targetMarker.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
        }

        if (targetMarker != null && firstBot != null && firstBot.IsDead)
        {
            targetMarker.gameObject.SetActive(false);
        }
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("player");
            player = playerObject != null ? playerObject.transform : null;
        }

        if (firstBot == null)
        {
            GameObject botObject = GameObject.Find("Bot-Body1");
            firstBot = botObject != null ? botObject.GetComponent<Health>() : null;
        }

        if (player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void CreateTutorialSigns()
    {
        if (player == null)
        {
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.forward;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 playerPosition = player.position;

        CreateSign(
            "TutorialSign_Controls",
            playerPosition + forward * 5.5f + right * -2.4f + Vector3.up * signHeight,
            "BASIC CONTROLS\nWASD move\nMouse look\nSpace jump\nC / Ctrl crouch",
            2.8f,
            1.35f);

        CreateSign(
            "TutorialSign_Stealth",
            playerPosition + forward * 10f + right * 2.6f + Vector3.up * signHeight,
            "STEALTH TIP\nCrouch near guards\nto lower suspicion.\nFollow the marker.",
            2.8f,
            1.25f);

        if (firstBot != null)
        {
            Vector3 botDirection = Vector3.ProjectOnPlane(playerPosition - firstBot.transform.position, Vector3.up).normalized;
            if (botDirection.sqrMagnitude < 0.01f)
            {
                botDirection = -forward;
            }

            CreateSign(
                "TutorialSign_FirstBot",
                firstBot.transform.position + botDirection * 2.8f + Vector3.up * signHeight,
                "FIRST BOT\nAim at the target.\nLeft click to fire.\nKill this Bot to finish Level 1.",
                3f,
                1.35f);
        }
    }

    private void CreateSign(string signName, Vector3 position, string text, float width, float height)
    {
        GameObject signRoot = new GameObject(signName);
        signRoot.transform.SetParent(transform, true);
        signRoot.transform.position = position;
        tutorialSigns.Add(signRoot.transform);

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

    private void CreateTargetMarker()
    {
        if (firstBot == null)
        {
            return;
        }

        targetMarkerBasePosition = firstBot.transform.position + Vector3.up * 2.1f;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "Tutorial_FirstBotMarker";
        marker.transform.SetParent(transform, true);
        marker.transform.position = targetMarkerBasePosition;
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

        targetMarker = marker.transform;
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
            Debug.LogWarning("Tutorial guide could not find a shader for runtime sign materials.");
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
