module Benchmark


#if INTERACTIVE
#load "../des/des.fsx"
#load "../des/tdea.fsx"
#load "../des/debug.fsx"
#r "nuget:xunit"
#endif

open des
open debug
open System
open System.Collections
open System.Diagnostics



let timed name fn arg =
    let timer = new Stopwatch()
    timer.Start()
    let ret = fn arg
    timer.Stop()
    printf "%-20s:%dms\n" name timer.ElapsedMilliseconds
    //let µs = (int ((float timer.ElapsedTicks) / 1000.0))
    //printf "%-20s:%dµs\n" name µs
    ret


[<EntryPoint>]
let main _ =
    let size = 100000 // 100Kb
    printfn "%s" "kitty"
    let key1 = genKey ()
    let key2 = genKey ()
    let key3 = genKey ()
    let iv = genKey ()
    let keys = (key1, key2, key3)


    let pt = timed "generation" randomBytes size

    let ans =
        pt
        |> timed "encrypting" (tdea.encryptBytes iv keys)
        |> timed "decrypting" (tdea.decryptBytes iv keys)

    assert (ans = pt)

    0
