using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camShakeScript : MonoBehaviour
{

    public IEnumerator camShake(float duration,float camShakeStrength,Vector3 direction)
    {
        float updatedShakeStrength = camShakeStrength;
        if (camShakeStrength > 10)
        {
            camShakeStrength = 10;
        }
        Vector3 originalPos = transform.position;
        Vector3 endPoint = new Vector3(direction.x, 0, direction.z)*(camShakeStrength/2);
        
        float timePassed = 0f;
        while (timePassed < duration)
        {

            float xPos = Random.Range(-.1f, .1f)*camShakeStrength;
            float zPos = Random.Range(-.1f, .1f)*camShakeStrength;
            Vector3 newPos = new Vector3(transform.position.x + xPos, transform.position.y, transform.position.z + zPos);
            //Vector3 newPos = endPoint + originalPos;
            transform.position = Vector3.Lerp(transform.position,newPos,0.15f);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }

        
           
            
    }
}
