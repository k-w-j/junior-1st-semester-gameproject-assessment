using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MummyRayAgent : Agent
{
    public Material goodMaterial;
    public Material badMaterial;
    private Renderer floorRenderer;
    private Material originMaterial;

    public float moveSpeed = 30f;
    public float turnSpeed = 100f;

    private ItemSpawner itemSpawner;

    public override void Initialize()
    {
        itemSpawner = transform.parent.GetComponent<ItemSpawner>();
        floorRenderer = transform.parent.Find("Floor").GetComponent<Renderer>();
        originMaterial = floorRenderer.material;
    }

    public override void OnEpisodeBegin()
    {
        itemSpawner.SpawnItems();
        transform.localPosition = new Vector3(Random.Range(-22f, 22f), 0.05f, Random.Range(-22f, 22f));
        transform.localRotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        Vector3 direction = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        switch (discreteActions[0])
        {
            case 0: direction = Vector3.zero; break;
            case 1: direction = Vector3.forward; break;
            case 2: direction = Vector3.back; break;
        }

        switch (discreteActions[1])
        {
            case 0: rotation = Vector3.zero; break;
            case 1: rotation = Vector3.up; break;
            case 2: rotation = Vector3.down; break;
        }

        transform.Rotate(rotation, turnSpeed * Time.fixedDeltaTime);
        transform.localPosition += direction * moveSpeed * Time.fixedDeltaTime;

        AddReward(-1 / (float)MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2;
        }
    }

    private IEnumerator ChangeFloorColor(Material changeMaterial)
    {
        floorRenderer.material = changeMaterial;
        yield return new WaitForSeconds(0.2f);
        floorRenderer.material = originMaterial;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("GOOD_ITEM"))
        {
            Destroy(collision.gameObject);
            AddReward(1f);
            StartCoroutine(ChangeFloorColor(goodMaterial));
        }

        if (collision.collider.CompareTag("BAD_ITEM"))
        {
            AddReward(-1f);
            EndEpisode();
            StartCoroutine(ChangeFloorColor(badMaterial));
        }

        //보상(-0.1)을 부여하고 에피소드를 종료.
        if (collision.collider.CompareTag("WALL"))
        {
            AddReward(-0.1f);
            EndEpisode();
            StartCoroutine(ChangeFloorColor(badMaterial));
        }
    }
}