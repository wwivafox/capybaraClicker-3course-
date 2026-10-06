using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    private Animator anim;
    private bool canPlayAnimation = false;


    void Start()
    {
        anim = GetComponent<Animator>();

        if (anim == null)
        {
            Debug.LogError("Ошибка: Animator не найден! Убедись, что компонент добавлен на капибару.");
            return;
        }
    }

   
    public void EnableRandomAnimation()
    {
        canPlayAnimation = true;
    }

   

    public void PlayRandomAnimation()
    {
        if (!canPlayAnimation)
        {
            Debug.Log("Анимация заблокирована (не готово)");
            return;
        }

        Debug.Log("Запускаем анимацию! Капибара не двигалась 30 секунд.");
        int randomValue = Random.Range(0, 100);
        int state;

        if (randomValue < 30)
            state = 1;
        else if (randomValue < 65)
            state = 2;
        else
            state = 3;

    
        anim.SetInteger("RandomState", state);
        anim.Play(state.ToString());
        canPlayAnimation = false;

        Debug.Log("Анимация запущена вручную: " + state);
        Debug.Log("После установки в Animator: " + anim.GetInteger("RandomState"));
    }



    private bool isScaling = false; 

    public void OnCapybaraClick()
    {
        if (ShopManeger.IsShopOpen || isScaling) return;

        StartCoroutine(ScaleCapybara());
    }

    IEnumerator ScaleCapybara()
    {
        isScaling = true; 

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f); 

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / duration);
            yield return null;
        }

        transform.localScale = originalScale; 
        isScaling = false; 
    }


}
