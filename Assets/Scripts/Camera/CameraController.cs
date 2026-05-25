using UnityEngine;
using System.Collections.Generic;

namespace Complete
{
    public class CameraController : MonoBehaviour
    {
        
            public float smoothTime = 0.2f;
            public float edgePadding = 4f;
            public float minZoom = 6.5f;

            private Camera cam;
            private float zoomVelocity;
            private Vector3 moveVelocity;
            private Vector3 targetPosition;
            private List<Transform> activePlayers = new List<Transform>();

            private void Awake()
            {
                cam = GetComponentInChildren<Camera>();
            }

            private void FixedUpdate()
            {
                RefreshPlayerList();
                if (activePlayers.Count == 0) return;

                MoveCamera();
                AdjustZoom();
            }

            private void RefreshPlayerList()
            {
                activePlayers.Clear();
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                foreach (GameObject player in players)
                {
                    if (player.activeSelf)
                        activePlayers.Add(player.transform);
                }
            }

            private void MoveCamera()
            {
                CalculateCenterPoint();
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, smoothTime);
            }

            private void CalculateCenterPoint()
            {
                Vector3 center = Vector3.zero;

                foreach (Transform player in activePlayers)
                    center += player.position;

                if (activePlayers.Count > 0)
                    center /= activePlayers.Count;

                center.y = transform.position.y;
                targetPosition = center;
            }

            private void AdjustZoom()
            {
                float neededSize = CalculateNeededSize();
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, neededSize, ref zoomVelocity, smoothTime);
            }

            private float CalculateNeededSize()
            {
                Vector3 centerLocal = transform.InverseTransformPoint(targetPosition);
                float size = 0f;

                foreach (Transform player in activePlayers)
                {
                    Vector3 playerLocal = transform.InverseTransformPoint(player.position);
                    Vector3 offset = playerLocal - centerLocal;

                    size = Mathf.Max(size, Mathf.Abs(offset.y));
                    size = Mathf.Max(size, Mathf.Abs(offset.x) / cam.aspect);
                }

                size += edgePadding;
                return Mathf.Max(size, minZoom);
            }
        
    }
}