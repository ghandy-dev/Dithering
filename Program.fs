open Arguments

open System
open System.IO
open System.Threading.Tasks

open Argu

open SkiaSharp

let loadImage (filepath: string) =
    task {
        try 
            let! bytes = File.ReadAllBytesAsync(filepath)
            return Some (SKBitmap.Decode(bytes))
        with 
        | :? System.IO.FileNotFoundException -> printfn $"""File "{filepath}" not found""" ; return None
        | ex -> printfn $"Failed to read file: {ex.Message}" ; return None
    }

let saveImage (bitmap: SKBitmap) (outputPath: string) =
    task {
        return!
            Task.Run(fun _ ->
                try
                    use stream = File.OpenWrite(outputPath)
                    let data = bitmap.Encode(SKEncodedImageFormat.Png, 100)
                    data.SaveTo(stream)
                    printfn $"File saved to %s{outputPath}"
                with ex ->
                    printfn $"Failed to save image: {ex.Message}"
            )
    }

let run (inputFilepath: string) (outputFilepath: string) (algo: SKBitmap -> unit) =
    task {
        match! loadImage inputFilepath with
        | None -> ()
        | Some bitmap -> 
            use _ = bitmap
            algo bitmap
            do! saveImage bitmap outputFilepath
    }

[<EntryPoint>]
let main args =
    task {
        let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
        let parser = ArgumentParser.Create<CliArguments>(programName = "ls", errorHandler = errorHandler)
        let results = parser.ParseCommandLine args

        let inputFile = results.GetResult(Input_File)
        let algorithm = defaultArg (results.TryGetResult(Dithering_Algorithm) |> Option.bind (fun d -> Some (parseAlgorithm d))) Dithering.floydSteinberg
        let outputPath = defaultArg (results.TryGetResult(Output_Path)) (Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
        let outputFile = 
            defaultArg (results.TryGetResult(Output_File)) $"""%s{Path.GetFileNameWithoutExtension(inputFile)}_%s{DateTime.Now.ToString("yyyyMMddHHmmss")}"""
            |> fun file -> Path.ChangeExtension(file, ".png")
        let outputFilepath = Path.Join(outputPath, outputFile)

        do! run inputFile outputFilepath algorithm
        
        return 0
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
