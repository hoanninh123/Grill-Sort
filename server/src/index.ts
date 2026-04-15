import cors from "cors";
import express from "express";
import { createServer } from "http";
import { Server } from "socket.io";
import { RoomService } from "./game/roomService";
import { registerHandlers } from "./socket/registerHandlers";

const app = express();
const httpServer = createServer(app);

const io = new Server(httpServer, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"],
  },
});

const roomService = new RoomService();
const port = Number(process.env.PORT ?? 3001);

// eslint-disable-next-line no-console
app.use(cors());
app.use(express.json());

// eslint-disable-next-line no-console
app.get("/health", (_req, res) => {
  res.json({
    ok: true,
    service: "grill-sort-realtime",
    now: new Date().toISOString(),
  });
});

// eslint-disable-next-line no-console
io.on("connection", (socket) => {
  registerHandlers(io, socket, roomService);
});

httpServer.listen(port, () => {
  // eslint-disable-next-line no-console
  console.log(`[grill-sort] socket server running on :${port}`);
});
