# Multiplayer Session Building Blocks

## How to utilize the Multiplayer Session Building Blocks

### Cloud Project Connection

- Make sure your Unity project is connected to a cloud project before use.
- Connect it to a project in the Unity editor via: `File > Project Settings > Services` (Select an organization and choose either an existing cloud project or create a new one)

### Scenes Overview

- **JoinByBrowsing**  
  Lets users log in and browse available sessions to join.  
  Ideal for standard authentication and finding lobbies.

- **JoinByCode**  
  UI that enables to create a session that other players can join using the created join code.  
  Perfect for joining and inviting players to a specific session through copying a code.

- **QuickJoin**  
  Automatically joins the first available session.  
  Perfect for quickly joining any existing session when matching with specific rules doesn't matter.

- **QuickJoinDebug**
  Same as QuickJoin just with additional session info window at the left with information about the session.
  Useful for debugging a session.

### Finding UI Elements

- The Kit contains some pre-made UI for your convenience inside the following folders:  
  `Assets / Blocks / MultiplayerSession / UI`  
  `Assets / Blocks / CommonSession / UI`
- When creating your own UI, you can find each Kit element in the UIBuilder editor.
    - Open UI Builder and click on the *Project* category inside the *Library* panel.
    - You can drag the elements from the Blocks section into the hierarchy in UI builder to add them to your own UI.
    - Sessions are being tracked between the Blocks VisualElements based on the `SessionType` value, don't forget to set this value in each element inspector after adding one to your UI.
- The UI elements are created through C# scripts utilizing the UXMLElement attribute. You can find the scripts in the **Runtime Folder** at:  
  `Assets / Blocks / MultiplayerSession / Runtime`  
  `Assets / Blocks / CommonSession / Runtime`
    - Each element is in its own folder and consists of a model view class and an element class.

### Session Type & Settings

- All UI elements communicate through the `SessionType`.
- Ensure all elements use the `SessionType` to work together.
- In our UXMLs, the session type is set in the `SessionSettings` scriptable object, which in turn is referenced by the uxml assets containing the UI.
