using UnityEngine;
using System.Collections.Generic;

namespace MonsterQuest.Combat
{
    public class WeaponOrbitVisualController : MonoBehaviour
    {
        [Header("Settings")]
        public float orbitSpeed = 5f;
        
        public void RealignWeapons(List<WeaponCompanionAgent> agents)
        {
            int count = agents.Count;
            if (count == 0) return;

            float zBack = -2.5f;

            for (int i = 0; i < count; i++)
            {
                var agent = agents[i];
                Vector3 targetOffset = Vector3.zero;
                float orbitTime = (Mathf.PI * 2f / count) * i;

                if (count == 1)
                {
                    targetOffset = new Vector3(0f, 1.0f, zBack);
                }
                else if (count == 2)
                {
                    float x = (i == 0) ? 0.6f : -0.6f;
                    targetOffset = new Vector3(x, 1.0f, zBack);
                }
                else if (count == 3)
                {
                    if (i == 0) targetOffset = new Vector3(0f, 1.5f, zBack);
                    else if (i == 1) targetOffset = new Vector3(0.8f, 0.4f, zBack);
                    else targetOffset = new Vector3(-0.8f, 0.4f, zBack);
                }
                else
                {
                    // Algoritmo de Capa de Espadas
                    int[] rows;
                    switch (count) {
                        case 4: rows = new int[] { 1, 3 }; break;
                        case 5: rows = new int[] { 2, 3 }; break;
                        case 6: rows = new int[] { 3, 3 }; break; 
                        case 7: rows = new int[] { 3, 4 }; break;
                        case 8: rows = new int[] { 4, 4 }; break;
                        case 9: rows = new int[] { 2, 3, 4 }; break; 
                        case 10: rows = new int[] { 3, 3, 4 }; break;
                        case 11: rows = new int[] { 3, 4, 4 }; break;
                        case 12: rows = new int[] { 3, 4, 5 }; break;
                        case 13: rows = new int[] { 4, 4, 5 }; break;
                        case 14: rows = new int[] { 4, 5, 5 }; break;
                        case 15: rows = new int[] { 4, 5, 6 }; break; 
                        default: rows = new int[] { count }; break;
                    }
                    
                    int rowIndex = 0;
                    int swordsBefore = 0;
                    for (int r = 0; r < rows.Length; r++) {
                        if (i < swordsBefore + rows[r]) {
                            rowIndex = r;
                            break;
                        }
                        swordsBefore += rows[r];
                    }
                    
                    int indexInRow = i - swordsBefore;
                    int swordsInRow = rows[rowIndex];
                    
                    float dx = 0.9f; 
                    float dy = 0.6f; 
                    
                    float totalHeight = (rows.Length - 1) * dy;
                    float startY = 1.0f + (totalHeight / 2f); 
                    
                    float distanceFromCenter = (indexInRow - (swordsInRow - 1) / 2.0f);
                    float x = distanceFromCenter * dx;
                    float y = startY - (rowIndex * dy);
                    
                    float zWingCurve = Mathf.Abs(distanceFromCenter) * 0.4f; 
                    float zRowOffset = rowIndex * 0.1f; 
                    
                    targetOffset = new Vector3(x, y, zBack - zRowOffset - zWingCurve);
                }

                agent.SetOrbitParameters(targetOffset, orbitTime, orbitSpeed);
            }
        }
    }
}
