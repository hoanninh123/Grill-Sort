# Grill-Sort

Puzzle game project with:
- Unity client (gameplay, UI, interactions)
- Node.js + TypeScript backend using Socket.IO (real-time features)

Demo:
- https://github.com/hoanninh123/Grill-Sort/issues/1

## Overview

Grill-Sort is a sorting puzzle game where players drag and place food items into valid slots and complete combinations. The project is split into two parts:

- Client: Unity project for rendering, drag-drop logic, animations, and local game flow.
- Backend: Socket.IO server for real-time communication such as matchmaking, session sync, live events, and player state updates.

## Tech Stack

- Client: Unity (C#)
- Backend: Node.js, TypeScript, Socket.IO

## High-Level Architecture

- Unity client sends player actions to backend via Socket.IO events.
- Backend validates/coordinates game state and broadcasts updates to connected players.
- Unity client listens for server events and updates UI/game state.

Example event flow:

1. Client connects -> `socket.connect`
2. Client joins room -> `room:join`
3. Client sends move -> `game:move`
4. Server validates/broadcasts -> `game:state`
5. Client renders new state

## Project Structure

Current repository mainly contains Unity source under `Assets/Scripts`.

Suggested backend folder (if not added yet):

```text
server/
	src/
		index.ts
		socket/
			handlers/
			events.ts
		game/
			room.service.ts
			game.service.ts
	package.json
	tsconfig.json
```

## Run Client (Unity)

1. Open project in Unity Editor.
2. Open the `Main` scene.
3. Ensure both `Main` and `Game` scenes are included in Build Settings.
4. Press Play in editor.

## Run Backend (Socket.IO + TypeScript)

From repository root:

```bash
npm install
npm run dev
```

The server starts at `http://localhost:3001` by default.

Build and run production bundle:

```bash
npm run build
npm run start
```

Available scripts:

```json
{
	"scripts": {
		"dev": "ts-node-dev --respawn --transpile-only server/src/index.ts",
		"build": "tsc -p server/tsconfig.json",
		"start": "node server/dist/index.js",
		"typecheck": "tsc -p server/tsconfig.json --noEmit"
	}
}
```

## Socket Events Contract

Client -> Server:

- `room:create`
  - payload: `{ "playerName": "Alice" }`
- `room:join`
  - payload: `{ "roomId": "123456", "playerName": "Bob" }`
- `game:move`
  - payload: `{ "roomId": "123456", "from": 0, "to": 1 }`
- `game:sync`
  - payload: `"123456"`

Server -> Client:

- `room:created`
- `room:joined`
- `room:players`
- `player:joined`
- `player:left`
- `game:state`
- `game:completed`
- `room:error`

## Unity Client Bridge

A Socket.IO Unity bridge script is available at:

- `Assets/Scripts/Network/SocketMultiplayerClient.cs`

Quick use:

1. Attach this component to a GameObject in your scene.
2. Keep server URL as `http://127.0.0.1:3001` for local testing.
3. Call `CreateRoom()` or `JoinRoom(roomId)` from your UI buttons.
4. Call `SendMove(from, to)` after each drag-drop move in gameplay.
5. Handle `game:state` payload to update grill state on screen.

## Notes

- Keep game logic authoritative on backend for multiplayer fairness.
- Use event versioning for backward compatibility between client and server.
- Add reconnect/resync flow (`socket.id` changes after reconnect).
