"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const cors_1 = __importDefault(require("cors"));
const express_1 = __importDefault(require("express"));
const http_1 = require("http");
const socket_io_1 = require("socket.io");
const roomService_1 = require("./game/roomService");
const registerHandlers_1 = require("./socket/registerHandlers");
const app = (0, express_1.default)();
const httpServer = (0, http_1.createServer)(app);
const io = new socket_io_1.Server(httpServer, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"],
    },
});
const roomService = new roomService_1.RoomService();
const port = Number(process.env.PORT ?? 3001);
app.use((0, cors_1.default)());
app.use(express_1.default.json());
app.get("/health", (_req, res) => {
    res.json({
        ok: true,
        service: "grill-sort-realtime",
        now: new Date().toISOString(),
    });
});
io.on("connection", (socket) => {
    (0, registerHandlers_1.registerHandlers)(io, socket, roomService);
});
httpServer.listen(port, () => {
    // eslint-disable-next-line no-console
    console.log(`[grill-sort] socket server running on :${port}`);
});
