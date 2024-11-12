open Arguments

open System
open System.IO
open System.Threading.Tasks

open Argu

open SkiaSharp

let loadImage (path: string) =
    task {
        let! bytes = File.ReadAllBytesAsync(path)
        return SKBitmap.Decode(bytes)
    }

let saveImage (bitmap: SKBitmap) (outputPath: string) =
    task {
        return!
            Task.Run(fun _ ->
                use stream = File.OpenWrite(outputPath)
                let data = bitmap.Encode(SKEncodedImageFormat.Png, 80)
                data.SaveTo(stream)
                printfn "File saved to %s" outputPath
            )
    }

let run (filepath: string) (outputPath: string) (algo: SKBitmap -> unit) =
    task {
        use! bitmap = loadImage filepath
        algo bitmap

        let filename =
            sprintf "%s\%s_%s.png"
                outputPath
                (Path.GetFileNameWithoutExtension(filepath))
                (DateTime.Now.ToString("yyyyMMddHHmmss"))

        do! saveImage bitmap filename
    }

[<EntryPoint>]
let main args =
    task {
        let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
        let parser = ArgumentParser.Create<CliArguments>(programName = "ls", errorHandler = errorHandler)
        let results = parser.ParseCommandLine args

        let filePath = results.GetResult(Input_File)
        let algorithm = defaultArg (results.TryGetResult(Dithering_Algorithm) |> Option.bind (fun d -> Some (parseAlgorithm d))) Dithering.floydSteinberg
        let outputPath = defaultArg (results.TryGetResult(Output_Path)) (Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))

        do! run filePath outputPath algorithm
        
        return 0
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
