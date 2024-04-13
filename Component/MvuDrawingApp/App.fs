namespace MvuDrawingApp

open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Markup.Xaml.Converters
open Avalonia.Media
open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia

open type Fabulous.Avalonia.View

open Fabulous.StackAllocatedCollections.StackList

[<AutoOpen>]
module EmptyBorderBuilders =
    type Fabulous.Avalonia.View with

        /// <summary>Creates an empty Border widget.</summary>
        static member EmptyBorder<'msg>() =
            WidgetBuilder<'msg, IFabBorder>(Border.WidgetKey, AttributesBundle(StackList.empty(), ValueNone, ValueNone))

module ColorPicker =

    type Model = { Color: Color }

    type Msg = PointerPressed of Color

    let init color () = { Color = color }

    let update msg model =
        match msg with
        | PointerPressed color -> { model with Color = color }

    let program (color: Color) = Program.stateful (init color) update

    let view color () =
        let brushes = [ Colors.Black; Colors.Red; Colors.Green; Colors.Blue; Colors.Yellow ]

        Component(program color) {
            let! model = Mvu.State

            HStack(5.) {
                for item in brushes do
                    View
                        .EmptyBorder()
                        .width(32.0)
                        .height(32.0)
                        .cornerRadius(16.0)
                        .background(SolidColorBrush(item))
                        .borderThickness(4.0)
                        .borderBrush(
                            if item = model.Color then
                                SolidColorBrush(item)
                            else
                                SolidColorBrush(Colors.Transparent)
                        )
                        .onPointerPressed(fun _ -> PointerPressed item)
            }
        }

module SizePicker =
    type Model = { Size: float }

    type Msg = PointerPressed of float

    let init () = { Size = 2. }, Cmd.none

    let update msg model =
        match msg with
        | PointerPressed size -> { Size = size }, Cmd.none

    let program = Program.statefulWithCmd init update

    let view () =
        let sizes = [ 2.; 4.; 6.; 8.; 16.; 32. ]

        Component(program) {
            let! model = Mvu.State

            HStack(5.) {
                for item in sizes do
                    View
                        .EmptyBorder()
                        .width(item)
                        .height(item)
                        .cornerRadius(item / 2.0)
                        .background(
                            if item = model.Size then
                                SolidColorBrush(Colors.Black)
                            else
                                SolidColorBrush(Colors.Gray)
                        )
                        .onPointerPressed(fun _ -> PointerPressed item)
            }
        }

module Setting =
    let view color =
        Component() {
            Border(
                Dock(false) {
                    ColorPicker.view color ()//).dock(Dock.Left)
                    SizePicker.view()//.dock(Dock.Right)
                }
            )
                .dock(Dock.Bottom)
                .margin(5.0)
                .padding(5.0)
                .cornerRadius(8.0)
                .background("#bdc3c7")
        }


module DrawingCanvas =
    type Model =
        { IsPressed: bool
          LastPoint: Point option
          Color: Color
          Size: float }

    type Msg =
        | PointerPressed of Input.PointerPressedEventArgs
        | PointerReleased of Input.PointerReleasedEventArgs
        | PointerMoved of Input.PointerEventArgs

    let init (color: Color) (size: float) () =
        { IsPressed = false
          LastPoint = None
          Color = color
          Size = size }

    let canvasRef = ViewRef<Canvas>()

    let update color msg model =
        match msg with
        | PointerPressed args ->
            let color = ColorPicker.update
            { model with IsPressed = true }
        | PointerReleased args -> { model with IsPressed = false }
        | PointerMoved args ->
            let point = args.GetPosition(canvasRef.Value)

            if model.IsPressed then
                match model.LastPoint with
                | Some lastPoint ->
                    let brush = unbox(ColorToBrushConverter.Convert(box(model.Color), typeof<IBrush>))

                    let line =
                        Shapes.Line(StartPoint = lastPoint, EndPoint = point, Stroke = brush, StrokeThickness = model.Size, StrokeLineCap = PenLineCap.Round)

                    if canvasRef.Value <> null then
                        canvasRef.Value.Children.Add(line)

                    { model with LastPoint = Some point }
                | None -> { model with LastPoint = Some point }
            else
                { model with LastPoint = Some point }

    let program color size = Program.stateful (init color size) (update color)

    let view color size =
        Component(program color size) {
            Canvas(canvasRef)
                .verticalAlignment(VerticalAlignment.Stretch)
                .horizontalAlignment(HorizontalAlignment.Stretch)
                .background(SolidColorBrush(Colors.White))
                .onPointerPressed(PointerPressed)
                .onPointerReleased(PointerReleased)
                .onPointerMoved(PointerMoved)
        }

module App =
    let content () =
        (Component() {
            Dock() {
                (Setting.view Colors.Black).dock(Dock.Bottom)
                (DrawingCanvas.view Colors.Black 10.).dock(Dock.Top)
            }
        })
            .background(SolidColorBrush(Colors.White))

    let view() =
#if MOBILE
        SingleViewApplication(content())
#else
        DesktopApplication(Window(content()))
#endif

    let create () =
        let theme () = FluentTheme()
        FabulousAppBuilder.Configure(theme, view)
