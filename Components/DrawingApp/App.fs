namespace DrawingApp

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

        /// <summary>Creates a empty Border widget.</summary>
        static member EmptyBorder<'msg>() =
            WidgetBuilder<'msg, IFabBorder>(Border.WidgetKey, AttributesBundle(StackList.empty(), ValueNone, ValueNone))


module App =
    let theme = FluentTheme()
    let canvasRef = ViewRef<Canvas>()

    let view =
        Component() {
            let! color = State(Colors.Transparent)
            let! size = State(2.)
            let! isPressed = State(false)
            let! lastPoint = State<Point option>(None)

            DesktopApplication(
                Window(
                    (Dock() {
                        Border(
                            Dock(false) {
                                let brushes = [ Colors.Black; Colors.Red; Colors.Green; Colors.Blue; Colors.Yellow ]

                                HStack(5.) {
                                    for item in brushes do
                                        View
                                            .EmptyBorder()
                                            .width(32.0)
                                            .height(32.0)
                                            .cornerRadius(16.0)
                                            .background(SolidColorBrush(item))
                                            .borderThickness(4.0)
                                            .dock(Dock.Left)
                                            .borderBrush(
                                                if item = color.Current then
                                                    SolidColorBrush(item)
                                                else
                                                    SolidColorBrush(Colors.Transparent)
                                            )
                                //.onPointerPressed(fun _ -> PointerPressed item)
                                }

                                let sizes = [ 2.; 4.; 6.; 8.; 16.; 32. ]

                                HStack(5.) {
                                    for item in sizes do
                                        View
                                            .EmptyBorder()
                                            .width(item)
                                            .height(item)
                                            .cornerRadius(item / 2.0)
                                            .dock(Dock.Right)
                                            .background(
                                                if item = size.Current then
                                                    SolidColorBrush(Colors.Black)
                                                else
                                                    SolidColorBrush(Colors.Gray)
                                            )
                                //.onPointerPressed(fun _ -> PointerPressed item)
                                }
                            }
                        )
                            .dock(Dock.Bottom)
                            .margin(5.0)
                            .padding(5.0)
                            .cornerRadius(8.0)
                            .background("#bdc3c7")
                            .dock(Dock.Bottom)

                        Canvas(canvasRef)
                            .verticalAlignment(VerticalAlignment.Stretch)
                            .horizontalAlignment(HorizontalAlignment.Stretch)
                            .background(SolidColorBrush(Colors.White))
                            .dock(Dock.Top)
                            .onPointerPressed(fun _ -> isPressed.Set(true))
                            .onPointerReleased(fun _ -> isPressed.Set(false))
                            .onPointerMoved2(fun args ->
                                let point = args.GetPosition(canvasRef.Value)

                                if isPressed.Current then
                                    match lastPoint.Current with
                                    | Some lp ->
                                        let brush = unbox(ColorToBrushConverter.Convert(box(color.Current), typeof<IBrush>))

                                        let line =
                                            Shapes.Line(
                                                StartPoint = lp,
                                                EndPoint = point,
                                                Stroke = brush,
                                                StrokeThickness = size.Current,
                                                StrokeLineCap = PenLineCap.Round
                                            )

                                        if canvasRef.Value <> null then
                                            canvasRef.Value.Children.Add(line)

                                        lastPoint.Set(Some lp)
                                    | None -> lastPoint.Set(Some point)
                                else
                                    lastPoint.Set(Some point))
                    })
                        .background(SolidColorBrush(Colors.White))
                )
            )
        }
