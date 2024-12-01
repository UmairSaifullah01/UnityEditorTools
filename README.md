
# Behviour Tree For Unity

A Behavior Tree (BT) in Unity structures AI using action nodes (tasks), condition nodes (checks), sequence nodes (ordered tasks), and selector nodes (alternative tasks) for efficient and dynamic decision-making.


## Installation

This library is distributed via Unity's built-in package manager. Required Unity 2018.3 or later.

```
https://github.com/UmairSaifullah01/BehaviorTree-Unity.git

```

### Unity Package
- Open Unity project
- Download and run .unitypackage file from the latest release  
## Usage

![Behavior Tree Node Views](https://raw.githubusercontent.com/UmairSaifullah01/Images/master/BehaviorTreeNodeViews.png)
## Example

Inherit from BehaviorTreeMonoRunner 
```csharp
public class AIController : BehaviorTreeMonoRunner
	{
    }
```
Call SetupTree() to initilized tree in Start or Awake
```csharp 
    void Start()
    {
        SetupTree();
    }

```
Create and function that you want to add in nodes and this function should return NodeState

```csharp
public NodeState IsEnemyInAttackRange()
		{
			NodeState nodeState = NodeState.FAILURE;
			
			if (Vector3.Distance(transform.position, target.position) <= attackRange)
			{
				
				nodeState = NodeState.SUCCESS;
				return nodeState;
			}

			
			return nodeState;
		}
```
## Features

- Find Fucntions with Reflection
- Live Preview of working nodes
- Working functions names are required
- Fully modifiable from Editor


## 🚀 About Me
Umair Saifullah ~ a unity developer from Pakistan.


## License

[MIT](https://github.com/UmairSaifullah01/BehaviorTree-Unity/blob/master/LICENSE)

