# car_RL_AI
A simple Reinforcement Learning Car AI using Proximal Policy Optimization
Implementation using Unity ML-agent


- [Training](#training)
- [Trained Result](#trained-result)
  - [Original Race Track](#trained-result)
  - [Random Race Track](#trained-result)
- [My Method](#method)

## Training
```
Step: 200000.
Time Elapsed: 628.180 s. 
Mean Reward: 48.482. 
Std of Reward: 59.216.
```
x3 speed training record 

![](images/training_r1.gif)

## Trained Result 
**Run the agent in different race tracks to test its performance** \
On original race track

![](images/train_result_final1.gif)

On random race track

![](images/train_result_final2.gif)

## Method
### Agent
**Number of observation states** : 7 , including 6 rays check the surrounds, return the distance between car and wall, another extra state store the angle between car and desired moving direction \
**Number of actions** : 2, forward speed and turning speed \
**Reward**: take the displacement between 2 frames, if car going toward desired direction, giving a positive reward proportional to the displacement, 0 if going backward, -1 if hit wall

![](images/car_agent.png)

Training Parameters
```
    trainer_type: ppo
    hyperparameters:
      batch_size: 10
      buffer_size: 100
      learning_rate: 3.0e-4
      beta: 5.0e-4
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
      learning_rate_schedule: linear
      beta_schedule: constant
      epsilon_schedule: linear
    network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: 500000
    time_horizon: 64
    summary_freq: 10000
```
