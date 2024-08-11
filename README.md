# Voodoo Draw.io Test

This project was developed by [Miguel Tomás](https://github.com/CoderGamester) for the exercise test created by Voodoo team.
This repo has been given open permissions to [Grabielle](https://github.com/GabrielleVoodo), [Benji](https://github.com/benjivoodoo) and [Miguel Tomé](https://github.com/mtome-mambo)

[![linkedin](https://img.shields.io/badge/linkedin-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/miguel-tomas)
[![github](https://img.shields.io/badge/github-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/CoderGamester)

## Roadmap

The development timeline of this project was also documented in the following [Trello Board](https://trello.com/b/ToWAnda7/drawio).

The [Trello Board](https://trello.com/b/ToWAnda7/drawio) is split into the following different sections:
- **Cleanup Column**: Initial cleanup done by me to organize the project structure for a potential increase of asset, plugins and code files into the `Assets` folder
- **Pre-production Column**: Project standards and architecture to allow the game to scale to multiple development teams while maintaining code robustness and guaranteeing good performance for the end user. Each card has a visual explanation of the standard in place. More details are below in the Architecture section. (All are proposed standards to be defined together with the team)
- **Data Test Column**: Contains all the code changes done to complete this exercise and meet the requirements. Including the Init loop to read/write the new data format, and the code changes to support the different social logins to authenticate the player into a mocked backend system. The social logins are invoked from the new buttons added to the settings screen
![Logo](https://trello.com/1/cards/66b8ee8f91d7f7a0b93d749f/attachments/66b8ee912c289c115f0f65bc/download/image.png)
- **Online persistence Column**: This is the proposal to use [Heroic Labs Solutions](https://heroiclabs.com/) to scale the game with LiveOps functionality configured from a server controlled by the game dev or product teams. Also, and more importantly, this online server will guarantee the persistence of authorized players (avoiding cheaters and hackers) to keep up their saved progress + execute all game logic from the server, following a server authoritative model.
- **Online Deterministic Column**: This is the proposal to use [Photon Quantum](https://www.photonengine.com/quantum) to guarantee determinism of the player's experience. This is only necessary if avoiding cheaters in the draw gameplay is paramount or lag compensation is an important feature. Due to the casual nature of the experience, a simple [state position event sync](state position event sync) from Heroic Labs is enough. Photon Quantum has an initial complex learning curve due to it's DSL custom language and critical memory management requirements similar to C++ coding, but this is something I can explain after 4 of development experience.
- **Tech Debt Column**: This is the proposal of the tech debt needed to cleanup before the project is ready for big scale. I ordered the work from top to bottom in terms of priority and marked the colours of urgency. RED = Urgent to do as soon as possible, YELLOW = Important to do and adjust future sprint work to include these changes while developing new features, GREEN = Good to do when a developer is already touching the same code base needed for some other changes

## Installation

Download or clone this project repo to run it.

The test-exercise was done using [Unity 2022.3.35f1](unityhub://2022.3.35f1/011206c7a712)

Start the project with the Scene **Main** instead of the scene ~~Game~~

## Packages used

```python
# Unity packages
com.unity.inputsystem
com.unity.ads.ios-support

# OSS packages
https://github.com/Cysharp/UniTask.git
https://github.com/CoderGamester/Services.git
https://github.com/CoderGamester/Unity-DataTypeExtensions.git

#For the social login, the game would need the following packages to work
https://github.com/lupidan/apple-signin-unity.git
https://github.com/playgameservices/play-games-plugin-for-unity
https://lookaside.facebook.com/developers/resources/?id=facebook-unity-sdk-17.0.0.zip
```

## Architecture

[Miro Board](https://miro.com/app/board/uXjVKsNbvlE=/?share_link_id=929681206943)

There is 2 proposed architectures in the [Trello Board](https://trello.com/b/ToWAnda7/drawio)
- **Non-Deterministic Architecture**: That doesn't use [Photon Quantum](https://www.photonengine.com/quantum) to process the gameplay logic
![Logo](https://trello.com/1/cards/66b12c89f259c379f1d177c5/attachments/66b51bf21702a6ef7a6625ea/previews/66b51bf21702a6ef7a6625f6/download/image.png)
- **Deterministic Architecture**: That uses [Photon Quantum](https://www.photonengine.com/quantum) to process the gameplay logic
![Logo](https://trello.com/1/cards/66b12c89f259c379f1d177c5/attachments/66b8989ec46831254873ccad/previews/66b8989fc46831254873ccb6/download/image.png)

The fundamentals of both architectures are the same and try to be as the most pure C# code base as possible. 
Both follow an [MVP architecture](https://unity.com/how-to/build-modular-codebase-mvc-and-mvp-programming-patterns) design to control the player's view and data flow into the game. To allow the model logic to run on the server in a simulation-type architecture, the Data and Logic are split in the model.

The main core of the architecture is to run the entire code flow through a Hierarchical-Finite-State-Machine (HFSM) or Statechart based on a public [repo](https://github.com/CoderGamester/Statechart-HFSM) project on GitHub
The glue of the architecture resides in the different Services, especially the Pub/Sub Message Broker service that is used to communicate between all game systems and controllers.

## Code-Architecture Init Loop

[Miro Board](https://miro.com/app/board/uXjVKsNbvlE=/?share_link_id=929681206943)

The following image explains the initialization of systems until the game starts. This was done in order to minimize the amount of work needed to change the current Managers developed. 
This flow is the one being executed in the test delivered. But I would suggest to change the Singleton Managers to the proposed Architecture in: [Game architecture](https://trello.com/c/clRqD5jc/4-architecture-to-run-online)

This would help the project follow SOLID coding principles, that in turn will allow for the right scalability and relability of the project. With modularized logic, distributed services and mockable interfaces, the entire code is ready to be testable and worked by different internal teams if necessary in the future.

In essence this would mean to replace all Singleton Managers with Game States, Services and Logic blocks. If the project require the management of many entities or a more deterministic online approach, then the gameplay level logic would also need to be migrated to ECS systems.

![Logo](https://trello.com/1/cards/66b89451d3ab23522fdce78a/attachments/66b898e36816ec719acba253/previews/66b898e46816ec719acba25c/download/image.png)

## Data structure

The data structure is configured in the `Scripts/Data/PlayerData.cs` file and saved in JSON format.
Following a Server-Authoritive model, this data structure would be manipulated directly in the server via commands invoked by the client (ex: EndGameResultsCommand). The Client would never save the data directly and would only request the data blob during the [player authentication process](https://trello.com/1/cards/66b89451d3ab23522fdce78a/attachments/66b898e36816ec719acba253/previews/66b898e46816ec719acba25c/download/image.png)

```csharp
using System;

namespace Game.Data
{
	/// <summary>
	/// Contains all the data in the scope of the Player 
	/// </summary>
	[Serializable]
	public class PlayerData
	{
		public int Level = 1;
		public int XP = 0;
		public string Nickname;
		public int BestScore = 0;
		public int FavoriteSkin = 0;
		public int[] GameResult = new int[5];
	}
}
```

## License

[MIT](https://choosealicense.com/licenses/mit/)