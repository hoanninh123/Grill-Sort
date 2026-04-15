export interface PlayerInfo {
  socketId: string;
  name: string;
}

export interface GameState {
  version: number;
  grills: string[][];
  isCompleted: boolean;
  updatedAt: string;
}

export interface Room {
  id: string;
  players: Map<string, PlayerInfo>;
  state: GameState;
}

export interface CreateRoomPayload {
  playerName?: string;
}

export interface JoinRoomPayload {
  roomId: string;
  playerName?: string;
}

export interface MovePayload {
  roomId: string;
  from: number;
  to: number;
}
