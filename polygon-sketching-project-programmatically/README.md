# Polygon Drawing Tool – Study Project

## Purpose
This application implements a simplified free-drawing mechanism for polygons on a canvas untilizing Konva (https://konvajs.org)-

Users add vertices by clicking, preview the next edge while moving the mouse, and finish a polygon with a double-click. Multiple polygons can be created and all actions support unlimited undo and redo.

The project focuses on **interaction logic and undo/redo design**, not graphical complexity.

---

## Core Functionality
- Click to add polygon vertices
- Live preview line from last vertex to mouse position
- Finish polygon via double-click
- Create multiple independent polygons
- Unlimited Undo / Redo
  - Buttons
  - Keyboard shortcuts (Ctrl + Z / Ctrl + Y)

---

## Central State Management
The application uses a **centrally managed mutable state**:

state
Holds the current application data used for rendering.

undoStack
Stores commands that have been executed and can be undone.

redoStack
Stores commands that were undone and can be reapplied.

## Design Pattern: Command

All user actions that modify the state are implemented using the Command Pattern.

Each command:

Represents a single user action

Encapsulates the logic to apply and revert that action

Exposes two functions:

do() – applies the change

undo() – reverts the change

## Key Functions (Conceptual Overview)

executeCommand(command)
Executes a command, stores it in the undo stack, clears the redo stack, and triggers a redraw.

undo()
Reverts the last executed command and moves it to the redo stack.

redo()
Reapplies the last undone command and moves it back to the undo stack.

redraw()
Clears the canvas and renders all polygons and current points from the central state.

## Double-Click Handling

Polygon completion uses manual double-click detection based on click timing.
This avoids unreliable native "dblclick" behavior on canvas elements and ensures consistent interaction across browsers.
Therefore the timing for the double click is configurable in the "Configuration" section of app.js

## Special Requirement: Non-funtional cheatsheet

Why This Is NOT a Functional Programming Approach

The application is not functional programming:

State is mutable and shared

Functions produce side effects (state mutation and canvas rendering)

Undo/Redo relies on imperative commands, not immutable state transformations

Application behavior depends on execution order and time
