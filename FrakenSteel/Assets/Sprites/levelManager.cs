using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelManager : MonoBehaviour
{
    public Animator transition;

    public float time = 2f;

    public void LoadLevel(int index){
        StartCoroutine(Transition(index));
    }

    IEnumerator Transition(int index){
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(index);
    }
}
