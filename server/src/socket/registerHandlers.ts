import { Server, Socket } from "socket.io";
import { RoomService } from "../game/roomService";
import { CreateRoomPayload, JoinRoomPayload, MovePayload, PlayerInfo } from "../game/types";

const DEFAULT_PLAYER_NAME = "Player";

function normalizeName(name?: string): string {
  const trimmed = (name ?? "").trim();
  return trimmed.length > 0 ? trimmed : DEFAULT_PLAYER_NAME;
}

function serializePlayers(roomService: RoomService, roomId: string): PlayerInfo[] {
  const room = roomService.getRoom(roomId);
  if (!room) {
    return [];
  }
  return Array.from(room.players.values());
}

export function registerHandlers(io: Server, socket: Socket, roomService: RoomService): void {
  socket.on("room:create", (payload: CreateRoomPayload = {}) => {
    const owner: PlayerInfo = {
      socketId: socket.id,
      name: normalizeName(payload.playerName),
    };

    const room = roomService.createRoom(owner);
    socket.join(room.id);

    socket.emit("room:created", {
      roomId: room.id,
      yourSocketId: socket.id,
      players: Array.from(room.players.values()),
      state: room.state,
    });
  });

  socket.on("room:join", (payload: JoinRoomPayload) => {
    if (!payload || !payload.roomId) {
      socket.emit("room:error", { message: "Missing roomId" });
      return;
    }

    const player: PlayerInfo = {
      socketId: socket.id,
      name: normalizeName(payload.playerName),
    };

    const room = roomService.joinRoom(payload.roomId, player);
    if (!room) {
      socket.emit("room:error", { message: "Room not found" });
      return;
    }

    socket.join(room.id);

    socket.emit("room:joined", {
      roomId: room.id,
      yourSocketId: socket.id,
      players: Array.from(room.players.values()),
      state: room.state,
    });

    socket.to(room.id).emit("player:joined", player);
    io.to(room.id).emit("game:state", room.state);
  });

  socket.on("game:move", (payload: MovePayload) => {
    try {
      if (!payload || !payload.roomId) {
        throw new Error("Missing roomId");
      }

      const state = roomService.applyMove(payload.roomId, payload.from, payload.to);
      io.to(payload.roomId).emit("game:state", state);

      if (state.isCompleted) {
        io.to(payload.roomId).emit("game:completed", {
          roomId: payload.roomId,
          state,
        });
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : "Move failed";
      socket.emit("room:error", { message });
    }
  });

  socket.on("game:sync", (roomId: string) => {
    if (!roomId) {
      socket.emit("room:error", { message: "Missing roomId" });
      return;
    }

    const room = roomService.getRoom(roomId);
    if (!room) {
      socket.emit("room:error", { message: "Room not found" });
      return;
    }

    socket.emit("game:state", room.state);
    socket.emit("room:players", serializePlayers(roomService, roomId));
  });

  socket.on("disconnect", () => {
    const room = roomService.removePlayer(socket.id);
    if (!room) {
      return;
    }

    io.to(room.id).emit("player:left", { socketId: socket.id });
    io.to(room.id).emit("room:players", Array.from(room.players.values()));
  });
}
