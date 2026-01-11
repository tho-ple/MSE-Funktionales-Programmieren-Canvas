// ==========================
// Configuration Double Click
// ==========================

const DOUBLE_CLICK_DELAY = 300; // ms
let lastClickTime = 0;

// ==========================
// Canvas Setup (Rendering)
// ==========================
const width = window.innerWidth;
const height = window.innerHeight;

const stage = new Konva.Stage({
  container: 'container',
  width: width,
  height: height,
});

const layer = new Konva.Layer();
stage.add(layer);

// ==========================
// Application State (Logic)
// ==========================
let state = {
  currentPoints: [],
  polygons: []
};

let undoStack = [];
let redoStack = [];

// Konva references (render-only)
let previewLine = null;

// ==========================
// Command System (Undo/Redo)
// ==========================
function executeCommand(command) {
  command.do();
  undoStack.push(command);
  redoStack = []; // clear redo on new action
  redraw();
}

function undo() {
  if (undoStack.length === 0) return;
  const command = undoStack.pop();
  command.undo();
  redoStack.push(command);
  redraw();
}

function redo() {
  if (redoStack.length === 0) return;
  const command = redoStack.pop();
  command.do();
  undoStack.push(command);
  redraw();
}

// ==========================
// Commands
// ==========================
function addPointCommand(point) {
  return {
    do: () => state.currentPoints.push(point),
    undo: () => state.currentPoints.pop()
  };
}

function finishPolygonCommand(points) {
  return {
    do: () => {
      state.polygons.push([...points]);
      state.currentPoints = [];
    },
    undo: () => {
      state.currentPoints = state.polygons.pop();
    }
  };
}

// ==========================
// Rendering (No Business Logic)
// ==========================
function redraw() {
  layer.destroyChildren();

  // Draw finished polygons
  state.polygons.forEach(poly => {
    layer.add(new Konva.Line({
      points: poly.flatMap(p => [p.x, p.y]),
      closed: true,
      fill: 'rgba(0,150,255,0.3)',
      stroke: 'black',
      strokeWidth: 2
    }));
  });

  // Draw current points
  state.currentPoints.forEach(p => {
    layer.add(new Konva.Circle({
      x: p.x,
      y: p.y,
      radius: 4,
      fill: 'red'
    }));
  });

  // Draw current polygon lines
  if (state.currentPoints.length >= 2) {
    layer.add(new Konva.Line({
      points: state.currentPoints.flatMap(p => [p.x, p.y]),
      stroke: 'blue',
      strokeWidth: 2
    }));
  }

  layer.draw();
}

// ==========================
// Mouse Preview Line
// ==========================
stage.on('mousemove', () => {
  if (state.currentPoints.length === 0) return;

  const pos = stage.getPointerPosition();
  if (previewLine) previewLine.destroy();

  const last = state.currentPoints[state.currentPoints.length - 1];

  previewLine = new Konva.Line({
    points: [last.x, last.y, pos.x, pos.y],
    stroke: 'gray',
    dash: [5, 5],
    strokeWidth: 1
  });

  layer.add(previewLine);
  layer.draw();
});

// ==========================
// Event Handling
// ==========================
stage.on('click', () => {
  const now = Date.now();
  const pos = stage.getPointerPosition();

  if (now - lastClickTime < DOUBLE_CLICK_DELAY) {
    // ---- DOUBLE CLICK ----
    if (state.currentPoints.length >= 3) {
      executeCommand(finishPolygonCommand(state.currentPoints));
    }
    lastClickTime = 0;
    return;
  }

  // ---- SINGLE CLICK ----
  lastClickTime = now;
  executeCommand(addPointCommand({ x: pos.x, y: pos.y }));
});

// ==========================
// Keyboard Shortcuts
// ==========================
document.addEventListener('keydown', (e) => {
  if (e.ctrlKey && e.key === 'z') {
    undo();
  }
  if (e.ctrlKey && (e.key === 'y' || e.key === 'Z')) {
    redo();
  }
});

// ==========================
// Optional UI Buttons
// ==========================
document.getElementById('undoBtn')?.addEventListener('click', undo);
document.getElementById('redoBtn')?.addEventListener('click', redo);
