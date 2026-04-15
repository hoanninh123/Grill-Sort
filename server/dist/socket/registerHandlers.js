"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.registerHandlers = registerHandlers;
const DEFAULT_PLAYER_NAME = "Player";
function normalizeName(name) {
    const trimmed = (name ?? "").trim();
    return trimmed.length > 0 ? trimmed : DEFAULT_PLAYER_NAME;
}
function serializePlayers(roomService, roomId) {
    const room = roomService.getRoom(roomId);
    if (!room) {
        return [];
    }
    return Array.from(room.players.values());
}
function registerHandlers(io, socket, roomService) {
    socket.on("room:create", (payload = {}) => {
        const owner = {
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
    socket.on("room:join", (payload) => {
        if (!payload || !payload.roomId) {
            socket.emit("room:error", { message: "Missing roomId" });
            return;
        }
        const player = {
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
    socket.on("game:move", (payload) => {
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
        }
        catch (error) {
            const message = error instanceof Error ? error.message : "Move failed";
            socket.emit("room:error", { message });
        }
    });
    socket.on("game:sync", (roomId) => {
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
