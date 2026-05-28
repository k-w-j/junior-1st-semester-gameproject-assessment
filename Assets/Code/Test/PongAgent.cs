using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class PongAgent : Agent
{
    public GameObject enemy;
    public GameObject ball;

    private Rigidbody RbAgent;
    private Rigidbody RbBall;

    private const int Stay = 0;
    private const int Up = 1;
    private const int Down = 2;

    private Vector3 ResetPosAgent;

    public override void Initialize()
    {
        ResetPosAgent = transform.localPosition;
        RbAgent = GetComponent<Rigidbody>();
        RbBall = ball.GetComponent<Rigidbody>();

        Academy.Instance.AgentPreStep += WaitTimeInference;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(enemy.transform.localPosition.z);
        sensor.AddObservation(ball.transform.localPosition.x);
        sensor.AddObservation(ball.transform.localPosition.z);
        sensor.AddObservation(RbBall.velocity.x);
        sensor.AddObservation(RbBall.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var vectorAction = actions.DiscreteActions;
        int action = Mathf.FloorToInt(vectorAction[0]);

        switch (action)
        {
            case Stay:
                transform.localPosition = transform.localPosition;
                break;
            case Up:
                transform.Translate(Vector3.forward * Time.fixedDeltaTime * 30f);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y,
                    Mathf.Clamp(transform.localPosition.z, -3.7f, 3.7f));
                break;
            case Down:
                transform.Translate(Vector3.back * Time.fixedDeltaTime * 30f);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y,
                    Mathf.Clamp(transform.localPosition.z, -3.7f, 3.7f));
                break;
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = ResetPosAgent;
        RbAgent.velocity = Vector3.zero;
        RbAgent.angularVelocity = Vector3.zero;
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }
    }


    float DecisionWaitingTime = 0.02f;
    float m_currentTime = 0f;
    public void WaitTimeInference(int action)
    {
        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }

        else
        {
            if (m_currentTime >= DecisionWaitingTime)
            {
                m_currentTime = 0;
                RequestDecision();
            }

            else
            {
                m_currentTime += Time.fixedDeltaTime;
            }
        }
    }

}
