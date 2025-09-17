using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopper : MonoBehaviour
{
    [SerializeField]
    private BonusController _controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision Test :     1");
        if (!_controller.isCollision)
        {
            Debug.Log("Collision Test :     1.5");
            _controller.isCollision = true;

            _controller.StopWheel();
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collision Test :     5");
        if (!_controller.isCollision)
        {
            Debug.Log("Collision Test :     5.5");
            _controller.isCollision = true;

            _controller.StopWheel();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Collision Test :     2");
        if (!_controller.isCollision)
        {
            Debug.Log("Collision Test :     2.5");
            _controller.isCollision = true;

            _controller.StopWheel();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("Collision Test :     3");
        if (!_controller.isCollision)
        {
            Debug.Log("Collision Test :     3.5");
            _controller.isCollision = true;

            _controller.StopWheel();
        }
    }
}
