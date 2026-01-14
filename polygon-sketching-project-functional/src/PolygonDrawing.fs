module PolygonDrawing 

open Fable.Core
open Feliz
open Elmish

type Coord = { x : float; y : float }

type PolyLine = list<Coord>

type Model = {
    finishedPolygons : list<PolyLine>
    currentPolygon : Option<PolyLine>
    mousePos : Option<Coord>
    past : list<Model>
    future : list<Model>
}

type Msg =
    | AddPoint of Coord
    | SetCursorPos of Option<Coord>
    | FinishPolygon
    | Undo
    | Redo

let init () =
    let m = 
        { finishedPolygons = []
          currentPolygon = None
          mousePos = None
          past = []
          future = [] }
    m, Cmd.none

let updateModel (msg : Msg) (model : Model) =
    match msg with
    | AddPoint coord ->
        match model.currentPolygon with
        | None -> 
            { model with currentPolygon = Some [coord] }
        | Some polygon ->
            { model with currentPolygon = Some (coord :: polygon) }
    
    | FinishPolygon ->
        match model.currentPolygon with
        | None -> 
            model
        | Some polygon when polygon.Length < 3 ->
            model
        | Some polygon ->
            { model with 
                finishedPolygons = polygon :: model.finishedPolygons
                currentPolygon = None }
    
    | _ -> model

let addUndoRedo (updateFunction : Msg -> Model -> Model) (msg : Msg) (model : Model) =
    match msg with
    | SetCursorPos p -> 
        { model with mousePos = p }
    
    | Undo -> 
        match model.past with
        | [] -> model
        | prevModel :: olderStates ->
            let currentSnapshot = 
                { finishedPolygons = model.finishedPolygons
                  currentPolygon = model.currentPolygon
                  mousePos = model.mousePos
                  past = []
                  future = [] }
            { prevModel with 
                past = olderStates
                future = currentSnapshot :: model.future }
    
    | Redo -> 
        match model.future with
        | [] -> model
        | nextModel :: newerStates ->
            let currentSnapshot = 
                { finishedPolygons = model.finishedPolygons
                  currentPolygon = model.currentPolygon
                  mousePos = model.mousePos
                  past = []
                  future = [] }
            { nextModel with 
                past = currentSnapshot :: model.past
                future = newerStates }
    
    | _ -> 
        let currentSnapshot = 
            { finishedPolygons = model.finishedPolygons
              currentPolygon = model.currentPolygon
              mousePos = model.mousePos
              past = []
              future = [] }
        let newModel = updateFunction msg model
        { newModel with 
            past = currentSnapshot :: model.past
            future = [] }

let update (msg : Msg) (model : Model)  =
    let newModel = addUndoRedo updateModel msg model
    newModel, Cmd.none

[<Emit("getSvgCoordinates($0)")>]
let getSvgCoordinates (o: Browser.Types.MouseEvent): Coord = jsNative

let viewPolygon (color : string) (isClosed : bool) (points : PolyLine) =
    let pairs = points |> List.pairwise
    let allPairs = 
        if isClosed && points.Length > 0 then
            let firstPoint = points |> List.last
            let lastPoint = points |> List.head
            pairs @ [(lastPoint, firstPoint)]
        else
            pairs
    
    allPairs |> List.map (fun (c0, c1) ->
        Svg.line [
            svg.x1 c0.x; svg.y1 c0.y
            svg.x2 c1.x; svg.y2 c1.y
            svg.stroke color
            svg.strokeWidth 2.0
            svg.strokeLineJoin "round"
        ]
    )

let viewPreviewLine (lastPoint : Coord) (mousePos : Coord) =
    Svg.line [
        svg.x1 lastPoint.x; svg.y1 lastPoint.y
        svg.x2 mousePos.x; svg.y2 mousePos.y
        svg.stroke "gray"
        svg.strokeWidth 1.0
    ]

let render (model : Model) (dispatch : Msg -> unit) =
    let border = 
        Svg.rect [
            svg.x 0; svg.y 0
            svg.width 500; svg.height 500
            svg.stroke "black"
            svg.strokeWidth 2
            svg.fill "none"
        ] 

    let finishedPolygons = 
        model.finishedPolygons 
        |> List.collect (viewPolygon "green" true)
    
    let currentPolygonLines =
        match model.currentPolygon with
        | None -> []
        | Some points -> viewPolygon "red" false points
    
    let previewLine =
        match model.currentPolygon, model.mousePos with
        | Some (lastPoint :: _), Some mousePos ->
            [viewPreviewLine lastPoint mousePos]
        | _ -> []
    
    let currentVertices =
        match model.currentPolygon with
        | None -> []
        | Some points ->
            points |> List.map (fun coord ->
                Svg.circle [
                    svg.cx coord.x
                    svg.cy coord.y
                    svg.r 4
                    svg.fill "red"
                ]
            )
 
    let svgElements = 
        List.concat [
            [border]
            finishedPolygons
            currentPolygonLines
            previewLine
            currentVertices
        ]

    Html.div [
        prop.style [style.custom("userSelect", "none")]
        prop.children [
            Html.h1 "Polygon Drawing Tool"
            Html.div [
                prop.style [style.marginBottom 10]
                prop.children [
                    Html.text "Click to add vertices. Double-click to finish polygon."
                ]
            ]
            Html.button [
                prop.style [style.margin 5]
                prop.disabled (model.past.IsEmpty)
                prop.onClick (fun _ -> dispatch Undo)
                prop.children [Html.text "Undo"]
            ]
            Html.button [
                prop.style [style.margin 5]
                prop.disabled (model.future.IsEmpty)
                prop.onClick (fun _ -> dispatch Redo)
                prop.children [Html.text "Redo"]
            ]
            Html.div [
                prop.style [style.marginTop 10; style.fontSize 12; style.color "gray"]
                prop.children [
                    Html.text (sprintf "Finished polygons: %d | Current points: %d" 
                        model.finishedPolygons.Length
                        (match model.currentPolygon with Some p -> p.Length | None -> 0))
                ]
            ]
            Html.br []
            Svg.svg [
                svg.width 500
                svg.height 500
                svg.onMouseMove (fun mouseEvent -> 
                    let pos = getSvgCoordinates mouseEvent
                    dispatch (SetCursorPos (Some pos))
                )
                svg.onMouseLeave (fun _ ->
                    dispatch (SetCursorPos None)
                )
                svg.onClick (fun mouseEvent -> 
                    if mouseEvent.detail = 1 then
                        let pos = getSvgCoordinates mouseEvent
                        dispatch (AddPoint pos)
                    elif mouseEvent.detail = 2 then
                        dispatch FinishPolygon
                )
                svg.children svgElements
            ]
        ]
    ]