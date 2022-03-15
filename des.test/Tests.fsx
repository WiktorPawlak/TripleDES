module Tests

#if INTERACTIVE
#load "../des/des.fsx"
#load "../des/tdea.fsx"
#load "../des/debug.fsx"
#r "nuget:xunit"
#endif

open des
open debug
open Xunit

[<Fact>]
let ``des enc/dec test`` () =
    let text = toBA "123456abcd132536"
    let key = toBA "aabb09182736ccdd"
    let expectedEncrypted = "c0b7a8d05f3a829c"
    let encrypted = des.encrypt key text
    let decrypted = des.decrypt key encrypted
    Assert.Equal(expectedEncrypted, toStr encrypted)
    Assert.Equal(toStr text, toStr decrypted)

[<Fact>]
let ``tdea działa`` () =
    let key1 = toBA "abb098376cd1ca2d"
    let key2 = toBA "a1b27ab60c89dd3c"
    let key3 = toBA "b0ba17a6298cd3cd"
    let keys = (key1, key2, key3)
    let plaintext = "c0b78d05af3a829c"

    let after =
        plaintext
        |> toBA
        |> tdea.encryptBlock keys
        |> tdea.decryptBlock keys
        |> toStr

    Assert.Equal(plaintext, after)
