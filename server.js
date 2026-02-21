'use strict';

const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const path = require('path');

const app = express();
const server = http.createServer(app);
const io = new Server(server);

app.use(express.static(path.join(__dirname, 'public')));

// ─── Game constants ────────────────────────────────────────────────────────────

const PHASES = { LOBBY: 'LOBBY', DESIGN: 'DESIGN', VIEWING: 'VIEWING', RESULTS: 'RESULTS' };
const DESIGN_SECONDS = 180;
const MIN_PLAYERS = 2;

const CHALLENGE_TOPICS = [
  'Luxurious Dining Area',
  'Cozy Living Room',
  'Modern Bedroom',
  'Home Office',
  "Artist's Studio",
  'Reading Nook',
  'Entertainment Room',
  'Zen Meditation Room',
  'Kids Playroom',
  'Master Suite',
];

// Bot pre‑built rooms (sparse 8×6 grids)
const BOT_ROOMS = [
  { wallColor: '#dce8f5', floorColor: '#c8a87a', cells: buildBotRoom1() },
  { wallColor: '#f5e6dc', floorColor: '#8b7355', cells: buildBotRoom2() },
];

function buildBotRoom1() {
  const g = emptyGrid();
  placeBot(g, 0, 0, 'window'); placeBot(g, 0, 3, 'window'); placeBot(g, 0, 6, 'window');
  placeBot(g, 1, 0, 'bookshelf'); placeBot(g, 1, 7, 'plant');
  placeBot(g, 2, 1, 'sofa'); placeBot(g, 2, 2, 'sofa'); placeBot(g, 2, 3, 'sofa');
  placeBot(g, 2, 6, 'lamp');
  placeBot(g, 3, 4, 'coffeetable');
  placeBot(g, 4, 0, 'armchair'); placeBot(g, 4, 7, 'armchair');
  placeBot(g, 5, 2, 'tv'); placeBot(g, 5, 3, 'tv'); placeBot(g, 5, 5, 'painting');
  return g;
}

function buildBotRoom2() {
  const g = emptyGrid();
  placeBot(g, 0, 1, 'window'); placeBot(g, 0, 5, 'window');
  placeBot(g, 1, 0, 'plant'); placeBot(g, 1, 7, 'mirror');
  placeBot(g, 2, 1, 'diningtable'); placeBot(g, 2, 2, 'diningtable');
  placeBot(g, 2, 3, 'diningtable'); placeBot(g, 2, 4, 'diningtable');
  placeBot(g, 3, 1, 'chair'); placeBot(g, 3, 4, 'chair');
  placeBot(g, 4, 3, 'vase'); placeBot(g, 4, 6, 'lamp');
  placeBot(g, 5, 0, 'fireplace'); placeBot(g, 5, 7, 'painting');
  return g;
}

function emptyGrid() {
  const rows = [];
  for (let r = 0; r < 6; r++) {
    rows.push(Array(8).fill(null));
  }
  return rows;
}

function placeBot(grid, row, col, id) {
  if (grid[row]) grid[row][col] = { id };
}

// ─── Game state ────────────────────────────────────────────────────────────────

let state = createFreshState();

function createFreshState() {
  return {
    phase: PHASES.LOBBY,
    players: {},        // socketId → { id, name, character, ready, room, score }
    topicVotes: {},     // topic → count
    challenge: null,
    viewingOrder: [],   // [socketId, ...]
    viewingIndex: 0,
    votes: {},          // voterId → { targetId → stars }
    designTimer: DESIGN_SECONDS,
    timerInterval: null,
  };
}

// ─── Helpers ───────────────────────────────────────────────────────────────────

function broadcast(event, data) { io.emit(event, data); }

function publicState() {
  return {
    phase: state.phase,
    players: Object.fromEntries(
      Object.entries(state.players).map(([id, p]) => [
        id,
        { id: p.id, name: p.name, character: p.character, ready: p.ready, score: p.score },
      ])
    ),
    topicVotes: state.topicVotes,
    challenge: state.challenge,
    viewingOrder: state.viewingOrder,
    viewingIndex: state.viewingIndex,
    designTimer: state.designTimer,
    votes: state.votes,
  };
}

function addBots() {
  const botNames = ['Alex Bot', 'Sam Bot'];
  const botCharacters = [
    { gender: 'female', skinTone: '#F1C27D', hairStyle: 'long', hairColor: '#8B4513', shirtStyle: 'blouse', shirtColor: '#e85d8a', bottomStyle: 'dress', bottomColor: '#c0392b', face: 2 },
    { gender: 'male', skinTone: '#C68642', hairStyle: 'short', hairColor: '#1a1a1a', shirtStyle: 'suit', shirtColor: '#2c3e50', bottomStyle: 'trousers', bottomColor: '#1a252f', face: 3 },
  ];
  botNames.forEach((name, i) => {
    const id = 'bot_' + i;
    state.players[id] = {
      id, name, character: botCharacters[i], ready: true, score: 0,
      room: { wallColor: '#dce8f5', floorColor: '#c8a87a', cells: BOT_ROOMS[i].cells },
      isBot: true,
    };
    broadcast('player_joined', { id, name, character: botCharacters[i] });
  });
}

function startDesignPhase() {
  // Pick challenge: top voted or random
  let challenge = CHALLENGE_TOPICS[Math.floor(Math.random() * CHALLENGE_TOPICS.length)];
  if (Object.keys(state.topicVotes).length > 0) {
    const sorted = Object.entries(state.topicVotes).sort((a, b) => b[1] - a[1]);
    challenge = sorted[0][0];
  }
  state.challenge = challenge;
  state.phase = PHASES.DESIGN;
  state.designTimer = DESIGN_SECONDS;

  // Initialise empty rooms for human players
  Object.values(state.players).forEach(p => {
    if (!p.isBot) {
      p.room = { wallColor: '#f0e6d3', floorColor: '#c8a87a', cells: emptyGrid() };
    }
  });

  broadcast('phase_change', { phase: PHASES.DESIGN, challenge, timer: DESIGN_SECONDS });

  state.timerInterval = setInterval(() => {
    state.designTimer -= 1;
    broadcast('timer_tick', { timer: state.designTimer });
    if (state.designTimer <= 0) {
      clearInterval(state.timerInterval);
      startViewingPhase();
    }
  }, 1000);
}

function startViewingPhase() {
  state.phase = PHASES.VIEWING;
  state.viewingOrder = Object.keys(state.players);
  state.viewingIndex = 0;
  state.votes = {};
  Object.keys(state.players).forEach(id => { state.votes[id] = {}; });

  broadcast('phase_change', {
    phase: PHASES.VIEWING,
    viewingOrder: state.viewingOrder,
    viewingIndex: 0,
    rooms: Object.fromEntries(
      Object.entries(state.players).map(([id, p]) => [id, p.room])
    ),
    players: Object.fromEntries(
      Object.entries(state.players).map(([id, p]) => [id, { id: p.id, name: p.name, character: p.character, score: p.score }])
    ),
    challenge: state.challenge,
  });

  // Auto-advance rooms where no human voter is eligible (e.g., player viewing own room solo)
  autoAdvanceIfNoVoters();
}

function autoAdvanceIfNoVoters() {
  const currentId = state.viewingOrder[state.viewingIndex];
  const humanIds  = Object.keys(state.players).filter(id => !state.players[id].isBot);
  const eligible  = humanIds.filter(id => id !== currentId);
  if (eligible.length === 0) {
    setTimeout(() => {
      if (state.phase === PHASES.VIEWING) advanceViewing();
    }, 1500);
  }
}

function advanceViewing() {
  state.viewingIndex += 1;
  if (state.viewingIndex >= state.viewingOrder.length) {
    tallyScores();
    startResults();
  } else {
    broadcast('next_room', { viewingIndex: state.viewingIndex });
    autoAdvanceIfNoVoters();
  }
}

function tallyScores() {
  // Sum up votes for each player
  const totals = {};
  Object.values(state.players).forEach(p => { totals[p.id] = 0; });
  Object.entries(state.votes).forEach(([, votesGiven]) => {
    Object.entries(votesGiven).forEach(([targetId, stars]) => {
      if (totals[targetId] !== undefined) totals[targetId] += stars;
    });
  });
  Object.values(state.players).forEach(p => { p.score = totals[p.id] || 0; });
}

function startResults() {
  state.phase = PHASES.RESULTS;
  const sorted = Object.values(state.players).sort((a, b) => b.score - a.score);
  const winner = sorted[0];
  broadcast('phase_change', {
    phase: PHASES.RESULTS,
    players: Object.fromEntries(
      Object.values(state.players).map(p => [p.id, { name: p.name, character: p.character, score: p.score, room: p.room }])
    ),
    winner: { id: winner.id, name: winner.name, character: winner.character, score: winner.score, room: winner.room },
    challenge: state.challenge,
  });
}

function checkVotingComplete(targetId) {
  // All human players (non‑bots) must have voted for the current room
  const humanIds = Object.keys(state.players).filter(id => !state.players[id].isBot);
  const currentRoomId = state.viewingOrder[state.viewingIndex];
  const votersForRoom = Object.entries(state.votes).filter(
    ([voterId, votesGiven]) => votesGiven[currentRoomId] !== undefined
  );
  // Voters shouldn't include the room's own player
  const eligibleVoters = humanIds.filter(id => id !== currentRoomId);
  return eligibleVoters.every(id => state.votes[id] && state.votes[id][currentRoomId] !== undefined);
}

// ─── Socket.io ─────────────────────────────────────────────────────────────────

io.on('connection', socket => {
  console.log('connected:', socket.id);

  socket.on('join_game', ({ name, character }) => {
    if (state.phase !== PHASES.LOBBY) {
      socket.emit('error_msg', 'Game already in progress.');
      return;
    }
    state.players[socket.id] = {
      id: socket.id, name, character, ready: false, score: 0, room: null, isBot: false,
    };
    broadcast('player_joined', { id: socket.id, name, character });
    socket.emit('game_state', publicState());
  });

  socket.on('vote_topic', ({ topic }) => {
    if (!CHALLENGE_TOPICS.includes(topic)) return;
    state.topicVotes[topic] = (state.topicVotes[topic] || 0) + 1;
    broadcast('topic_votes_update', { topicVotes: state.topicVotes });
  });

  socket.on('player_ready', () => {
    if (!state.players[socket.id]) return;
    state.players[socket.id].ready = true;
    broadcast('player_ready', { id: socket.id });

    const humanPlayers = Object.values(state.players).filter(p => !p.isBot);
    if (humanPlayers.length >= 1 && humanPlayers.every(p => p.ready)) {
      if (humanPlayers.length < MIN_PLAYERS) addBots();
      if (state.timerInterval) clearInterval(state.timerInterval);
      startDesignPhase();
    }
  });

  socket.on('room_update', ({ room }) => {
    if (!state.players[socket.id]) return;
    state.players[socket.id].room = room;
    // Broadcast to others so they can peek (optional real-time preview)
    socket.broadcast.emit('player_room_update', { id: socket.id, room });
  });

  socket.on('submit_room', ({ room }) => {
    if (!state.players[socket.id]) return;
    state.players[socket.id].room = room;
    state.players[socket.id].ready = true;
  });

  socket.on('cast_vote', ({ targetId, stars }) => {
    if (!state.players[socket.id]) return;
    if (state.phase !== PHASES.VIEWING) return;
    if (!state.votes[socket.id]) state.votes[socket.id] = {};
    state.votes[socket.id][targetId] = Math.max(1, Math.min(5, stars));
    broadcast('vote_cast', { voterId: socket.id, targetId, stars: state.votes[socket.id][targetId] });

    if (checkVotingComplete(targetId)) {
      setTimeout(advanceViewing, 1000);
    }
  });

  socket.on('play_again', () => {
    if (state.timerInterval) clearInterval(state.timerInterval);
    state = createFreshState();
    broadcast('game_reset', {});
  });

  socket.on('disconnect', () => {
    console.log('disconnected:', socket.id);
    if (state.players[socket.id]) {
      const name = state.players[socket.id].name;
      delete state.players[socket.id];
      broadcast('player_left', { id: socket.id, name });
    }
  });
});

// ─── Start ─────────────────────────────────────────────────────────────────────

const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
  console.log(`🏠  Impress My Guests! running at http://localhost:${PORT}`);
});

// Dev route: skip design timer (only available in development, not production)
if (process.env.NODE_ENV !== 'production') {
  app.post('/dev/skip-design', (req, res) => {
    if (state.phase === PHASES.DESIGN) {
      if (state.timerInterval) clearInterval(state.timerInterval);
      startViewingPhase();
      res.json({ ok: true });
    } else {
      res.json({ ok: false, phase: state.phase });
    }
  });
}
