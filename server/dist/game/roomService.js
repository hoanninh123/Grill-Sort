"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RoomService = void 0;
const DEFAULT_GRILL_COUNT = 6;
const DEFAULT_GRILL_CAPACITY = 4;
const FOOD_TYPES = ["beef", "chicken", "shrimp", "corn", "fish", "sausage"];
class RoomService {
    constructor() {
        this.rooms = new Map();
    }
    createRoom(owner) {
        const roomId = this.generateRoomId();
        const room = {
            id: roomId,
            players: new Map([[owner.socketId, owner]]),
            state: this.createInitialState(),
        };
        this.rooms.set(roomId, room);
        return room;
    }
    getRoom(roomId) {
        return this.rooms.get(roomId);
    }
    joinRoom(roomId, player) {
        const room = this.rooms.get(roomId);
        if (!room) {
            return null;
        }
        room.players.set(player.socketId, player);
        return room;
    }
    removePlayer(socketId) {
        for (const room of this.rooms.values()) {
            if (!room.players.has(socketId)) {
                continue;
            }
            room.players.delete(socketId);
            if (room.players.size === 0) {
                this.rooms.delete(room.id);
            }
            return room;
        }
        return null;
    }
    applyMove(roomId, from, to) {
        const room = this.rooms.get(roomId);
        if (!room) {
            throw new Error("Room not found");
        }
        const grills = room.state.grills;
        this.validateMove(grills, from, to);
        const source = grills[from];
        const destination = grills[to];
        const item = source.pop();
        if (!item) {
            throw new Error("Invalid move: source grill is empty");
        }
        destination.push(item);
        room.state.version += 1;
        room.state.updatedAt = new Date().toISOString();
        room.state.isCompleted = this.computeIsCompleted(room.state.grills);
        return room.state;
    }
    validateMove(grills, from, to) {
        if (from === to) {
            throw new Error("Invalid move: source and destination are the same");
        }
        if (!Number.isInteger(from) || !Number.isInteger(to)) {
            throw new Error("Invalid move: grill index must be an integer");
        }
        if (from < 0 || from >= grills.length || to < 0 || to >= grills.length) {
            throw new Error("Invalid move: grill index out of range");
        }
        if (grills[from].length === 0) {
            throw new Error("Invalid move: source grill is empty");
        }
        if (grills[to].length >= DEFAULT_GRILL_CAPACITY) {
            throw new Error("Invalid move: destination grill is full");
        }
    }
    createInitialState() {
        const sequence = [];
        for (let i = 0; i < DEFAULT_GRILL_COUNT - 1; i += 1) {
            const food = FOOD_TYPES[i % FOOD_TYPES.length];
            for (let j = 0; j < DEFAULT_GRILL_CAPACITY; j += 1) {
                sequence.push(food);
            }
        }
        this.shuffle(sequence);
        const grills = Array.from({ length: DEFAULT_GRILL_COUNT }, () => []);
        for (let i = 0; i < sequence.length; i += 1) {
            const grillIndex = i % (DEFAULT_GRILL_COUNT - 1);
            grills[grillIndex].push(sequence[i]);
        }
        return {
            version: 1,
            grills,
            isCompleted: false,
            updatedAt: new Date().toISOString(),
        };
    }
    computeIsCompleted(grills) {
        return grills.every((grill) => {
            if (grill.length === 0) {
                return true;
            }
            if (grill.length !== DEFAULT_GRILL_CAPACITY) {
                return false;
            }
            return grill.every((item) => item === grill[0]);
        });
    }
    shuffle(arr) {
        for (let i = arr.length - 1; i > 0; i -= 1) {
            const j = Math.floor(Math.random() * (i + 1));
            const temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }
    generateRoomId() {
        let roomId = "";
        do {
            roomId = Math.floor(100000 + Math.random() * 900000).toString();
        } while (this.rooms.has(roomId));
        return roomId;
    }
}
exports.RoomService = RoomService;
