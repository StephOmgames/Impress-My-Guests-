'use strict';
/* ═══════════════════════════════════════════════════════════════
   Impress My Guests! – Game Client
   ═══════════════════════════════════════════════════════════════ */

// ─── Constants ────────────────────────────────────────────────────────────────

const CHALLENGE_TOPICS = [
  'Luxurious Dining Area', 'Cozy Living Room', 'Modern Bedroom', 'Home Office',
  "Artist's Studio", 'Reading Nook', 'Entertainment Room', 'Zen Meditation Room',
  'Kids Playroom', 'Master Suite',
];

const FURNITURE_CATALOG = {
  Windows: [
    { id: 'window',   name: 'Window',    emoji: '🪟' },
    { id: 'curtains', name: 'Curtains',  emoji: '🏮' },
  ],
  Seating: [
    { id: 'sofa',     name: 'Sofa',      emoji: '🛋️' },
    { id: 'armchair', name: 'Armchair',  emoji: '💺' },
    { id: 'chair',    name: 'Chair',     emoji: '🪑' },
  ],
  Tables: [
    { id: 'diningtable',  name: 'Dining Table',  emoji: '🍽️' },
    { id: 'coffeetable',  name: 'Coffee Table',  emoji: '☕' },
    { id: 'desk',         name: 'Desk',          emoji: '🖥️' },
  ],
  Decor: [
    { id: 'plant',    name: 'Plant',     emoji: '🪴' },
    { id: 'lamp',     name: 'Lamp',      emoji: '💡' },
    { id: 'painting', name: 'Painting',  emoji: '🖼️' },
    { id: 'mirror',   name: 'Mirror',    emoji: '🪞' },
    { id: 'vase',     name: 'Vase',      emoji: '🏺' },
    { id: 'rug',      name: 'Rug',       emoji: '🔲' },
  ],
  Entertainment: [
    { id: 'tv',         name: 'TV',          emoji: '📺' },
    { id: 'bookshelf',  name: 'Bookshelf',   emoji: '📚' },
    { id: 'fireplace',  name: 'Fireplace',   emoji: '🔥' },
    { id: 'piano',      name: 'Piano',       emoji: '🎹' },
  ],
  Bedroom: [
    { id: 'bed',       name: 'Bed',         emoji: '🛏️' },
    { id: 'nightstand',name: 'Nightstand',  emoji: '🕯️' },
    { id: 'wardrobe',  name: 'Wardrobe',    emoji: '👔' },
  ],
};

// Build a flat id→definition lookup once at module load (O(n), not repeated)
const FURNITURE_BY_ID = Object.fromEntries(
  Object.values(FURNITURE_CATALOG).flat().map(f => [f.id, f])
);

const STAR_LABELS = ['', 'Needs Work', 'Decent', 'Good', 'Great!', 'Stunning! ✨'];
const ROWS = 6, COLS = 8;

// ─── State ────────────────────────────────────────────────────────────────────

let socket;
let myId = null;
let myName = '';
let myChar = {
  gender: 'female',
  skinTone: '#FFDBB4',
  face: 1,
  hairStyle: 'short',
  hairColor: '#1a1a1a',
  shirtStyle: 'tshirt',
  shirtColor: '#3b82f6',
  bottomStyle: 'trousers',
  bottomColor: '#374151',
};

let roomGrid = emptyGrid();
let selectedFurniture = null;
let eraseMode = false;
let wallColor = '#dce8f5';
let floorColor = '#c8a87a';
let currentPaletteCategory = 'Windows';

let gamePhase = 'LOBBY';
let challenge = '';
let viewingOrder = [];
let viewingIndex = 0;
let allRooms = {};
let allPlayers = {};
let myVotes = {};
let selectedStars = 0;
let votedForCurrent = false;
let topicVoted = false;

// ─── Helpers ──────────────────────────────────────────────────────────────────

function emptyGrid() {
  return Array.from({ length: ROWS }, () => Array(COLS).fill(null));
}

function showScreen(id) {
  document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
  const el = document.getElementById('screen-' + id);
  if (el) el.classList.add('active');
}

function showToast(msg, dur = 2800) {
  const t = document.getElementById('toast');
  t.textContent = msg;
  t.classList.add('show');
  clearTimeout(t._timer);
  t._timer = setTimeout(() => t.classList.remove('show'), dur);
}

function fmtTimer(s) {
  const m = Math.floor(s / 60);
  const sec = s % 60;
  return `${m}:${sec.toString().padStart(2, '0')}`;
}

// ─── Character rendering ──────────────────────────────────────────────────────

function applyCharToWrap(wrap, char) {
  if (!wrap || !char) return;
  // Hair/skin
  const hairBack = wrap.querySelector('.char-hair-back');
  const hairFront = wrap.querySelector('.char-hair-front');
  const head      = wrap.querySelector('.char-head');
  const shirt     = wrap.querySelector('.char-shirt');
  const bottom    = wrap.querySelector('.char-bottom');

  if (hairBack) hairBack.style.background = char.hairColor || '#1a1a1a';
  if (hairFront) hairFront.style.background = char.hairColor || '#1a1a1a';
  if (head) head.style.background = char.skinTone || '#FFDBB4';
  if (shirt) shirt.style.background = char.shirtColor || '#3b82f6';
  if (bottom) bottom.style.background = char.bottomColor || '#374151';

  // Remove all style classes from wrap
  wrap.className = 'char-wrap';

  // Hair style class
  wrap.classList.add('hair-' + (char.hairStyle || 'short'));

  // Face class
  wrap.classList.add('face-' + (char.face || 1));

  // Shirt style class
  wrap.classList.add('shirt-' + (char.shirtStyle || 'tshirt'));

  // Bottom style class
  const bottomStyleClass = char.gender === 'male' ? 'bottom-trousers' : 'bottom-' + (char.bottomStyle || 'trousers');
  wrap.classList.add(bottomStyleClass);
}

function buildCharWrap(char) {
  const wrap = document.createElement('div');
  wrap.className = 'char-wrap';
  wrap.innerHTML = `
    <div class="char-hair-back"></div>
    <div class="char-head">
      <div class="char-eyes">
        <div class="eye left-eye"><div class="pupil"></div></div>
        <div class="eye right-eye"><div class="pupil"></div></div>
      </div>
      <div class="char-nose"></div>
      <div class="char-mouth"></div>
    </div>
    <div class="char-hair-front"></div>
    <div class="char-shirt"></div>
    <div class="char-bottom"></div>
  `;
  applyCharToWrap(wrap, char);
  return wrap;
}

function updatePreview() {
  const wrap = document.getElementById('char-wrap');
  if (wrap) applyCharToWrap(wrap, myChar);
  const nameTag = document.getElementById('char-name-tag');
  if (nameTag) nameTag.textContent = myName || 'Your Character';
}

// ─── Room grid ────────────────────────────────────────────────────────────────

function buildRoomGrid(container, grid, wc, fc, interactive) {
  container.innerHTML = '';
  container.style.background = wc || '#f0e6d3';
  for (let r = 0; r < ROWS; r++) {
    for (let c = 0; c < COLS; c++) {
      const cell = document.createElement('div');
      cell.className = 'room-cell';
      cell.style.background = fc || '#c8a87a';
      cell.dataset.row = r;
      cell.dataset.col = c;

      const item = grid[r][c];
      if (item) {
        const fd = FURNITURE_BY_ID[item.id];
        if (fd) {
          cell.textContent = fd.emoji;
          cell.classList.add('occupied');
          const lbl = document.createElement('span');
          lbl.className = 'cell-label';
          lbl.textContent = fd.name;
          cell.appendChild(lbl);
        }
      }

      if (interactive) {
        cell.addEventListener('click', () => handleCellClick(r, c));
      }

      container.appendChild(cell);
    }
  }
}

function handleCellClick(r, c) {
  if (eraseMode) {
    roomGrid[r][c] = null;
    refreshDesignGrid();
    emitRoomUpdate();
    return;
  }
  if (selectedFurniture) {
    if (roomGrid[r][c] && roomGrid[r][c].id === selectedFurniture.id) {
      roomGrid[r][c] = null; // toggle off
    } else {
      roomGrid[r][c] = { id: selectedFurniture.id };
    }
    refreshDesignGrid();
    emitRoomUpdate();
  }
}

function refreshDesignGrid() {
  const grid = document.getElementById('room-grid');
  if (grid) buildRoomGrid(grid, roomGrid, wallColor, floorColor, true);
}

function emitRoomUpdate() {
  if (socket) socket.emit('room_update', { room: { wallColor, floorColor, cells: roomGrid } });
}

// ─── Palette ──────────────────────────────────────────────────────────────────

function buildPalette() {
  const cats = document.getElementById('palette-cats');
  const items = document.getElementById('palette-items');
  if (!cats || !items) return;

  cats.innerHTML = '';
  Object.keys(FURNITURE_CATALOG).forEach(cat => {
    const btn = document.createElement('button');
    btn.className = 'cat-btn' + (cat === currentPaletteCategory ? ' active' : '');
    btn.textContent = cat;
    btn.addEventListener('click', () => {
      currentPaletteCategory = cat;
      document.querySelectorAll('.cat-btn').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      renderPaletteItems();
    });
    cats.appendChild(btn);
  });

  renderPaletteItems();
}

function renderPaletteItems() {
  const items = document.getElementById('palette-items');
  if (!items) return;
  items.innerHTML = '';
  (FURNITURE_CATALOG[currentPaletteCategory] || []).forEach(f => {
    const el = document.createElement('div');
    el.className = 'palette-item' + (selectedFurniture && selectedFurniture.id === f.id ? ' selected' : '');
    el.innerHTML = `<span class="fi">${f.emoji}</span><span class="fl">${f.name}</span>`;
    el.addEventListener('click', () => selectFurniture(f));
    items.appendChild(el);
  });
}

function selectFurniture(f) {
  selectedFurniture = f;
  eraseMode = false;
  document.getElementById('btn-erase').classList.remove('active');
  renderPaletteItems();
}

// ─── Lobby ────────────────────────────────────────────────────────────────────

function renderLobby(players, topicVotes) {
  const container = document.getElementById('lobby-players');
  if (!container) return;
  container.innerHTML = '';
  Object.values(players).forEach(p => {
    const card = document.createElement('div');
    card.className = 'lobby-player-card';
    const charDiv = document.createElement('div');
    charDiv.className = 'lobby-player-char';
    charDiv.appendChild(buildCharWrap(p.character));
    card.appendChild(charDiv);
    const name = document.createElement('span');
    name.className = 'lobby-player-name';
    name.textContent = p.name;
    card.appendChild(name);
    if (p.ready) {
      const ready = document.createElement('span');
      ready.className = 'lobby-player-ready';
      ready.textContent = ' ✅';
      card.appendChild(ready);
    }
    container.appendChild(card);
  });

  // Topic votes
  const topicList = document.getElementById('topic-list');
  if (topicList && topicVotes) {
    topicList.querySelectorAll('.topic-btn').forEach(btn => {
      const topic = btn.dataset.topic;
      const count = topicVotes[topic] || 0;
      btn.querySelector('.topic-vote-count').textContent = count;
      btn.classList.toggle('has-votes', count > 0);
    });
  }

  const status = document.getElementById('lobby-status');
  const count = Object.values(players).filter(p => !p.isBot).length;
  if (status) status.textContent = `${count} player${count !== 1 ? 's' : ''} in lobby`;
}

function buildTopicButtons() {
  const list = document.getElementById('topic-list');
  if (!list) return;
  list.innerHTML = '';
  CHALLENGE_TOPICS.forEach(topic => {
    const btn = document.createElement('button');
    btn.className = 'topic-btn';
    btn.dataset.topic = topic;
    btn.innerHTML = `${topic}<span class="topic-vote-count">0</span>`;
    btn.addEventListener('click', () => {
      if (topicVoted) return;
      topicVoted = true;
      document.querySelectorAll('.topic-btn').forEach(b => b.classList.remove('voted'));
      btn.classList.add('voted');
      socket.emit('vote_topic', { topic });
    });
    list.appendChild(btn);
  });
}

// ─── Viewing / Voting ─────────────────────────────────────────────────────────

function renderViewingRoom(index) {
  const targetId = viewingOrder[index];
  const player   = allPlayers[targetId];
  const room     = allRooms[targetId];

  if (!player || !room) return;

  // Progress
  const prog = document.getElementById('viewing-progress');
  if (prog) prog.textContent = `Room ${index + 1} of ${viewingOrder.length}`;

  // Player info
  const vpName = document.getElementById('vp-name');
  if (vpName) vpName.textContent = player.name + (targetId === myId ? ' (your room)' : '');

  const vpChar = document.getElementById('vp-char');
  if (vpChar) {
    vpChar.innerHTML = '';
    vpChar.appendChild(buildCharWrap(player.character));
  }

  // Room
  const grid = document.getElementById('viewing-room-grid');
  if (grid) buildRoomGrid(grid, room.cells, room.wallColor, room.floorColor, false);

  // Reset vote UI — disable voting on your own room
  const isOwnRoom = (targetId === myId);
  selectedStars = 0;
  votedForCurrent = isOwnRoom || (myVotes[targetId] !== undefined);
  renderStars(0);
  const btn = document.getElementById('btn-cast-vote');
  if (btn) {
    btn.disabled = votedForCurrent;
    btn.textContent = isOwnRoom ? 'Your Room' : 'Cast Vote ★';
  }
  const label = document.getElementById('star-label');
  if (label) label.textContent = isOwnRoom ? 'You cannot vote on your own room' : 'Select a rating';
  if (!isOwnRoom && myVotes[targetId] !== undefined) {
    selectedStars = myVotes[targetId];
    renderStars(selectedStars, true);
  }

  // Clear previous votes display
  const vd = document.getElementById('all-votes-display');
  if (vd) vd.innerHTML = '';
}

function renderStars(hoveredIndex, lock = false) {
  document.querySelectorAll('#star-rating .star').forEach((star, i) => {
    star.classList.toggle('lit', i < hoveredIndex);
  });
  const label = document.getElementById('star-label');
  if (label) label.textContent = hoveredIndex ? STAR_LABELS[hoveredIndex] : 'Select a rating';
  if (!lock) selectedStars = hoveredIndex;
}

function showVoteCast(voterId, targetId, stars) {
  const currentId = viewingOrder[viewingIndex];
  if (targetId !== currentId) return;
  const voter = allPlayers[voterId];
  if (!voter) return;
  const vd = document.getElementById('all-votes-display');
  if (!vd) return;
  // Avoid duplicates
  const existing = vd.querySelector(`[data-voter="${voterId}"]`);
  const entry = existing || document.createElement('div');
  entry.className = 'vote-entry';
  entry.dataset.voter = voterId;
  entry.innerHTML = `<span>${voter.name}</span><span class="vote-stars">${'★'.repeat(stars)}</span>`;
  if (!existing) vd.appendChild(entry);
}

// ─── Results ──────────────────────────────────────────────────────────────────

function showResults(data) {
  showScreen('results');

  const winner = data.winner;
  const wName  = document.getElementById('winner-name');
  const wScore = document.getElementById('winner-score');
  const wChar  = document.getElementById('winner-char');
  const wRoom  = document.getElementById('winner-room-grid');
  const wTitle = document.getElementById('winner-room-title');

  if (wName)  wName.textContent  = winner.name;
  if (wScore) wScore.textContent = `⭐ ${winner.score} total stars`;
  if (wTitle) wTitle.textContent = `${winner.name}'s Winning Room — "${data.challenge}"`;

  if (wChar) {
    wChar.innerHTML = '';
    wChar.appendChild(buildCharWrap(winner.character));
  }

  if (wRoom && winner.room) {
    buildRoomGrid(wRoom, winner.room.cells, winner.room.wallColor, winner.room.floorColor, false);
  }

  // Scoreboard
  const sorted = Object.values(data.players).sort((a, b) => b.score - a.score);
  const list = document.getElementById('score-list');
  if (list) {
    list.innerHTML = '';
    sorted.forEach((p, i) => {
      const li = document.createElement('li');
      li.className = 'score-entry';
      const rankClass = i === 0 ? 'gold' : i === 1 ? 'silver' : i === 2 ? 'bronze' : '';
      li.innerHTML = `
        <span class="score-rank ${rankClass}">#${i + 1}</span>
        <span class="score-player-name">${p.name}</span>
        <span class="score-stars">${'★'.repeat(p.score)} (${p.score})</span>
      `;
      list.appendChild(li);
    });
  }
}

// ─── Mini player list in design screen ───────────────────────────────────────

function updateMiniPlayerList() {
  const container = document.getElementById('player-list-mini');
  if (!container) return;
  container.innerHTML = '';
  Object.values(allPlayers).forEach(p => {
    const el = document.createElement('div');
    el.className = 'mini-player';
    el.textContent = p.name + (p.id === myId ? ' (you)' : '');
    container.appendChild(el);
  });
}

// ─── Socket setup ─────────────────────────────────────────────────────────────

function initSocket() {
  socket = io();

  socket.on('connect', () => { myId = socket.id; });

  socket.on('game_state', (state) => {
    allPlayers   = state.players || {};
    gamePhase    = state.phase;
    challenge    = state.challenge || '';
    viewingOrder = state.viewingOrder || [];
    viewingIndex = state.viewingIndex || 0;

    if (gamePhase === 'LOBBY') {
      renderLobby(allPlayers, state.topicVotes);
    }
  });

  socket.on('player_joined', ({ id, name, character }) => {
    allPlayers[id] = { id, name, character, ready: false, score: 0 };
    renderLobby(allPlayers, null);
    showToast(`${name} joined!`);
  });

  socket.on('player_ready', ({ id }) => {
    if (allPlayers[id]) allPlayers[id].ready = true;
    renderLobby(allPlayers, null);
  });

  socket.on('player_left', ({ id, name }) => {
    delete allPlayers[id];
    renderLobby(allPlayers, null);
    showToast(`${name} left.`);
  });

  socket.on('topic_votes_update', ({ topicVotes }) => {
    renderLobby(allPlayers, topicVotes);
  });

  socket.on('timer_tick', ({ timer }) => {
    const el = document.getElementById('design-timer');
    if (el) {
      el.textContent = fmtTimer(timer);
      el.classList.toggle('urgent', timer <= 30);
    }
  });

  socket.on('player_room_update', ({ id, room }) => {
    allRooms[id] = room;
  });

  socket.on('phase_change', (data) => {
    gamePhase = data.phase;

    if (data.phase === 'DESIGN') {
      challenge = data.challenge;
      document.getElementById('challenge-title').textContent = challenge;
      document.getElementById('challenge-badge').style.display = '';
      document.getElementById('design-timer').textContent = fmtTimer(data.timer || 180);
      buildPalette();
      refreshDesignGrid();
      updateMiniPlayerList();
      showScreen('design');
      showToast(`Challenge: "${challenge}" — 3 minutes to design!`, 4000);
    }

    if (data.phase === 'VIEWING') {
      allRooms     = data.rooms || {};
      viewingOrder = data.viewingOrder || [];
      viewingIndex = data.viewingIndex || 0;
      challenge    = data.challenge || challenge;
      myVotes      = {};
      // Merge all players (including bots) sent with viewing phase
      if (data.players) Object.assign(allPlayers, data.players);

      document.getElementById('viewing-challenge-title').textContent = challenge;
      renderViewingRoom(viewingIndex);
      showScreen('viewing');
    }

    if (data.phase === 'RESULTS') {
      showResults(data);
    }
  });

  socket.on('next_room', ({ viewingIndex: idx }) => {
    viewingIndex = idx;
    renderViewingRoom(viewingIndex);
    showToast('Next room!');
  });

  socket.on('vote_cast', ({ voterId, targetId, stars }) => {
    showVoteCast(voterId, targetId, stars);
    if (voterId === myId) {
      myVotes[targetId] = stars;
      const btn = document.getElementById('btn-cast-vote');
      if (btn) btn.disabled = true;
    }
  });

  socket.on('game_reset', () => {
    allPlayers = {}; allRooms = {}; myVotes = {};
    roomGrid = emptyGrid(); topicVoted = false; selectedStars = 0;
    showScreen('welcome');
    showToast('New game starting!');
  });

  socket.on('error_msg', (msg) => { showToast('⚠️ ' + msg, 4000); });
}

// ─── Event wiring ─────────────────────────────────────────────────────────────

function wireWelcomeScreen() {
  const nameInput = document.getElementById('player-name');
  const btnNext   = document.getElementById('btn-to-character');

  nameInput.addEventListener('input', () => {
    myName = nameInput.value.trim();
    btnNext.disabled = myName.length < 2;
  });

  btnNext.addEventListener('click', () => {
    if (myName.length < 2) return;
    updatePreview();
    showScreen('character');
  });
}

function wireCharacterScreen() {
  // Gender
  document.querySelectorAll('[data-gender]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-gender]').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      myChar.gender = btn.dataset.gender;
      // Reset bottom style for gender
      if (myChar.gender === 'male') {
        myChar.bottomStyle = 'trousers';
        document.querySelectorAll('[data-bottom]').forEach(b => b.classList.remove('active'));
        const first = document.querySelector('[data-bottom="trousers"]');
        if (first) first.classList.add('active');
      }
      updateBottomOptions();
      updatePreview();
    });
  });

  // Skin tone
  document.querySelectorAll('[data-skin]').forEach(sw => {
    sw.addEventListener('click', () => {
      document.querySelectorAll('[data-skin]').forEach(s => s.classList.remove('active'));
      sw.classList.add('active');
      myChar.skinTone = sw.dataset.skin;
      updatePreview();
    });
  });

  // Face
  document.querySelectorAll('[data-face]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-face]').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      myChar.face = parseInt(btn.dataset.face);
      updatePreview();
    });
  });

  // Hair style
  document.querySelectorAll('[data-hairstyle]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-hairstyle]').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      myChar.hairStyle = btn.dataset.hairstyle;
      updatePreview();
    });
  });

  // Hair color
  document.querySelectorAll('[data-hair]').forEach(sw => {
    sw.addEventListener('click', () => {
      document.querySelectorAll('[data-hair]').forEach(s => s.classList.remove('active'));
      sw.classList.add('active');
      myChar.hairColor = sw.dataset.hair;
      updatePreview();
    });
  });

  // Shirt style
  document.querySelectorAll('[data-shirt]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-shirt]').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      myChar.shirtStyle = btn.dataset.shirt;
      updatePreview();
    });
  });

  // Shirt color
  document.querySelectorAll('[data-shirtcolor]').forEach(sw => {
    sw.addEventListener('click', () => {
      document.querySelectorAll('[data-shirtcolor]').forEach(s => s.classList.remove('active'));
      sw.classList.add('active');
      myChar.shirtColor = sw.dataset.shirtcolor;
      updatePreview();
    });
  });

  // Bottom style
  document.querySelectorAll('[data-bottom]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-bottom]').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      myChar.bottomStyle = btn.dataset.bottom;
      updatePreview();
    });
  });

  // Bottom color
  document.querySelectorAll('[data-bottomcolor]').forEach(sw => {
    sw.addEventListener('click', () => {
      document.querySelectorAll('[data-bottomcolor]').forEach(s => s.classList.remove('active'));
      sw.classList.add('active');
      myChar.bottomColor = sw.dataset.bottomcolor;
      updatePreview();
    });
  });

  // Enter lobby
  document.getElementById('btn-to-lobby').addEventListener('click', () => {
    initSocket();
    socket.emit('join_game', { name: myName, character: myChar });
    buildTopicButtons();
    showScreen('lobby');
  });
}

function updateBottomOptions() {
  const btns = document.getElementById('bottom-btns');
  if (!btns) return;
  if (myChar.gender === 'male') {
    btns.innerHTML = `
      <button class="btn btn-toggle active" data-bottom="trousers">Trousers</button>
      <button class="btn btn-toggle" data-bottom="pencilskirt">Dress Pants</button>
    `;
  } else {
    btns.innerHTML = `
      <button class="btn btn-toggle active" data-bottom="trousers">Trousers</button>
      <button class="btn btn-toggle" data-bottom="skirt">Skirt</button>
      <button class="btn btn-toggle" data-bottom="dress">Dress</button>
      <button class="btn btn-toggle" data-bottom="pencilskirt">Pencil Skirt</button>
    `;
  }
  // Re-wire these new buttons
  btns.querySelectorAll('[data-bottom]').forEach(btn => {
    btn.addEventListener('click', () => {
      btns.querySelectorAll('[data-bottom]').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      myChar.bottomStyle = btn.dataset.bottom;
      updatePreview();
    });
  });
}

function wireLobbyScreen() {
  document.getElementById('btn-ready').addEventListener('click', () => {
    socket.emit('player_ready');
    document.getElementById('btn-ready').disabled = true;
    document.getElementById('btn-ready').textContent = '⏳ Waiting for others…';
  });
}

function wireDesignScreen() {
  document.getElementById('btn-erase').addEventListener('click', () => {
    eraseMode = !eraseMode;
    selectedFurniture = null;
    document.getElementById('btn-erase').classList.toggle('active', eraseMode);
    if (eraseMode) {
      document.querySelectorAll('.palette-item').forEach(el => el.classList.remove('selected'));
    }
  });

  document.getElementById('wall-color-picker').addEventListener('input', e => {
    wallColor = e.target.value;
    refreshDesignGrid();
    emitRoomUpdate();
  });

  document.getElementById('floor-color-picker').addEventListener('input', e => {
    floorColor = e.target.value;
    refreshDesignGrid();
    emitRoomUpdate();
  });

  document.getElementById('btn-submit-room').addEventListener('click', () => {
    socket.emit('submit_room', { room: { wallColor, floorColor, cells: roomGrid } });
    document.getElementById('btn-submit-room').textContent = '✅ Submitted!';
    document.getElementById('btn-submit-room').disabled = true;
    showToast('Room submitted! Waiting for others…');
  });
}

function wireViewingScreen() {
  // Stars
  document.querySelectorAll('#star-rating .star').forEach((star, i) => {
    star.addEventListener('mouseover', () => { if (!votedForCurrent) renderStars(i + 1); });
    star.addEventListener('mouseout',  () => { if (!votedForCurrent) renderStars(selectedStars); });
    star.addEventListener('click',     () => {
      if (votedForCurrent) return;
      selectedStars = i + 1;
      renderStars(selectedStars, true);
      document.getElementById('btn-cast-vote').disabled = false;
    });
  });

  document.getElementById('btn-cast-vote').addEventListener('click', () => {
    if (!selectedStars || votedForCurrent) return;
    const targetId = viewingOrder[viewingIndex];
    socket.emit('cast_vote', { targetId, stars: selectedStars });
    votedForCurrent = true;
    myVotes[targetId] = selectedStars;
    document.getElementById('btn-cast-vote').disabled = true;
    showToast(`Voted ${selectedStars} ⭐ for this room!`);
  });
}

function wireResultsScreen() {
  document.getElementById('btn-play-again').addEventListener('click', () => {
    socket.emit('play_again');
  });
}

// ─── Init ─────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
  wireWelcomeScreen();
  wireCharacterScreen();
  wireLobbyScreen();
  wireDesignScreen();
  wireViewingScreen();
  wireResultsScreen();

  // Initialise room grid (visible in design screen)
  buildRoomGrid(
    document.getElementById('room-grid') || document.createElement('div'),
    roomGrid, wallColor, floorColor, true
  );

  showScreen('welcome');
});
