using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyArrowManager : MonoBehaviour
{
    public Camera mainCam;
    public RectTransform canvasRect;
    public RectTransform arrowTemplate;
    public float screenEdgeBuffer = 50f;
    public float fadeSpeed = 3f;

    private class ArrowData
    {
        public RectTransform arrowTransform;
        public CanvasGroup canvasGroup;
    }

    private Dictionary<Transform, ArrowData> enemyArrows = new();

    void Update()
    {
        List<Transform> toRemove = new();
        foreach (var kvp in enemyArrows)
        {
            if (kvp.Key == null)
            {
                Destroy(kvp.Value.arrowTransform.gameObject);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove) enemyArrows.Remove(key);

        foreach (Transform enemy in EnemyManager.Instance.activeEnemies)
        {
            if (!enemyArrows.ContainsKey(enemy))
            {
                RectTransform newArrow = Instantiate(arrowTemplate, canvasRect);
                newArrow.gameObject.SetActive(true);

                CanvasGroup cg = newArrow.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = newArrow.gameObject.AddComponent<CanvasGroup>();

                cg.alpha = 0;

                enemyArrows.Add(enemy, new ArrowData
                {
                    arrowTransform = newArrow,
                    canvasGroup = cg
                });
            }

            ArrowData arrowData = enemyArrows[enemy];
            RectTransform arrow = arrowData.arrowTransform;
            CanvasGroup canvasGroup = arrowData.canvasGroup;

            Vector3 screenPos = mainCam.WorldToScreenPoint(enemy.position);
            if (screenPos.z < 0)
            {
                screenPos *= -1; // Mirror the position to flip direction
            }

            bool isOffscreen = screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;

            if (isOffscreen)
            {
                Vector3 screenCenter = new(Screen.width / 2f, Screen.height / 2f, 0);
                Vector3 dir = (screenPos - screenCenter).normalized;

                Vector3 edgePos = screenCenter + dir * ((Screen.height / 2f) - screenEdgeBuffer);
                edgePos.x = Mathf.Clamp(edgePos.x, screenEdgeBuffer, Screen.width - screenEdgeBuffer);
                edgePos.y = Mathf.Clamp(edgePos.y, screenEdgeBuffer, Screen.height - screenEdgeBuffer);

                arrow.position = edgePos;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                arrow.rotation = Quaternion.Euler(0, 0, angle - 90);

                // Fade in
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
            }
            else
            {
                // Fade out
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
    }
}