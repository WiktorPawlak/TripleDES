module Tests

#if INTERACTIVE
#load "../des/des.fsx"
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
    let encrypted = crypt.encryptBlock key text
    let decrypted = crypt.decryptBlock key encrypted
    Assert.Equal(expectedEncrypted, toStr encrypted)
    Assert.Equal(toStr text, toStr decrypted)
