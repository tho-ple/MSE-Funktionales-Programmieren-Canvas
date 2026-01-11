# Polygon Drawing template

This repository is based on the [Feliz](https://github.com/Zaid-Ajaj/Feliz) and uses [Fable](http://fable.io/) for transpiling F# code to JS.

The template is used in this semester in masters course "Functional Programming (by Harald Steinlechner)" @ University of Applied Sciences | Technikum Wien for studying UI applications from a functional programming perspective.

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) v8.0 or higher
* [node.js](https://nodejs.org) v18+ LTS


## Editor

To write and edit your code, you can use either VS Code + [Ionide](http://ionide.io/), Emacs with [fsharp-mode](https://github.com/fsharp/emacs-fsharp-mode), [Rider](https://www.jetbrains.com/rider/) or Visual Studio.


## Development

Before doing anything, start with installing npm dependencies using `npm install`.

Then to start development mode with hot module reloading, run:
```bash
npm start
```
This will start the development server after compiling the project, once it is finished, navigate to http://localhost:5173 to view the application .

To build the application and make ready for production:
```
npm run build
```
This command builds the application and puts the generated files into the `dist` directory.
