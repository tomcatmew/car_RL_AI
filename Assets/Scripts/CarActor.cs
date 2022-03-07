using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// This is simple reinforcement learning Car agent
// By Yifei Chen 2022/3/8


public class CarActor : Agent
{
    [SerializeField]private float speed = 10f;
    [SerializeField]private float turn_force = 1f;
    [SerializeField]private float total_score = 0.0f;
    //[SerializeField]private float speed_bonus_scale = 1.0f;

    private Transform current_block;
    private Transform initial_state;

    public bool toggle_ray = false;
    public override void Initialize()
    {
        initial_state = transform;
        block_dir_update();
        //total_score += reward;
    }

    private void CarControl(float turn, float forward, float dt)
    {
        float dis = speed * forward;
        transform.Translate(dis * dt * Vector3.forward);

        float rotation = turn_force * turn * 90f;
        transform.Rotate(0f, rotation * dt, 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float turn = actions.ContinuousActions[0];
        float forward = actions.ContinuousActions[1];

        var lastPos = transform.position;
        float dt = Time.fixedDeltaTime;   
        CarControl(turn, forward, dt);
        block_dir_update();
        var ds = transform.position - lastPos;
        float angle = Vector3.Angle(ds, current_block.forward);
        // higher speed higher reward
        float speed_bonus = Mathf.Clamp01(forward) * dt;
        // if car follow the path, positive reward, vice versa
        float reward = (1f - angle / 90f) * speed_bonus;
        total_score += reward;
        AddReward(reward);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    public override void CollectObservations(VectorSensor vectorSensor)
    {
        float angle = Vector3.SignedAngle(current_block.forward, transform.forward, Vector3.up);
        // - 180 to + 180, check car orientation 
        vectorSensor.AddObservation(angle / 180f);

        // ray shoot to the left side and rights side
        //vectorSensor.AddObservation(ShootRay(.1f, .5f, 70f,2f));
        //vectorSensor.AddObservation(ShootRay(.1f, -.5f, -70f, 2f));
        
        // 4 rays, front 3 rays, back 1 ray
        vectorSensor.AddObservation(ShootRay(1.0f, .5f, 25f, 5f));
        vectorSensor.AddObservation(ShootRay(1.5f, 0f, 0f, 5f));
        vectorSensor.AddObservation(ShootRay(1.0f, -.5f, -25f, 5f));
        vectorSensor.AddObservation(ShootRay(-1.5f, 0, 180f, 5f));
    }

    private float ShootRay(float offfset_z, float offset_x, float angle, float length)
    {
        var car_pos = transform;

        var ray_origin = car_pos.position + Vector3.up / 2f;
        var position = ray_origin + car_pos.forward * offfset_z + car_pos.right * offset_x;

        var euler_Angle = Quaternion.Euler(0f, angle, 0f);
        var dir = euler_Angle * car_pos.forward;

        if (toggle_ray == true)
        {
            Debug.DrawRay(position, dir * length, Color.red);
        }
        Physics.Raycast(position, dir, out var hit, length);

        // observation based on the distance between wall
        return hit.distance >= 0 ? hit.distance / length : -1f;
    }

    private void block_dir_update()
    {
        // we want to the car follow the path 

        var car_pose = transform.position + new Vector3(0f,0.05f,0f);

        if(toggle_ray)
            Debug.DrawRay(car_pose, Vector3.down * 2f, Color.red);
        if (Physics.Raycast(car_pose, Vector3.down, out var hit, 1f))
        {
            // check the new block
            var new_block = hit.transform;
            current_block = new_block;
        }
    }

    public override void OnEpisodeBegin()
    {
        //transform.localPosition = new Vector3(4.0f, 0.05f, -4.0f);
        //transform.localRotation = Quaternion.identity;
        transform.localPosition = initial_state.position;
        transform.localRotation = initial_state.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("wall"))
        {
            // if hit wall, shutdown simulation
            SetReward(-1f);
            EndEpisode();
        }
    }
    void OnGUI()
    {

        GUI.Box(new Rect(10, 10, 200, 90), "Loader Menu");

        if (GUI.Button(new Rect(20, 60, 160, 20), "Toggle Ray Display"))
        {
            if (toggle_ray == true)
                toggle_ray = false;
            else
                toggle_ray = true;
        }
    }
}
