module SecurityTests

open System
open System.Data
open System.IO
open Xunit
open Auth;


let generateRandomString length =
    let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
    let random = Random()
    String(Array.init length (fun _ -> chars[random.Next(chars.Length)]))

// Generate a list of random strings with lengths between 8 and 26
let generateTestData count =
    let random = Random()
    List.init count (fun _ -> 
        let length = random.Next(8, 26) // Random length between 8 and 26
        generateRandomString length
    )

// Example: Create 10 random strings

[<Fact>]
let ``HashedPasswords Hashes Passwords`` () =
    let testData = generateTestData 100
    testData |> List.iter ( fun password ->
                let hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor=4)
                Assert.NotEqual<string>(password, hashedPassword)
                )


[<Fact>]
let ``Checked Passwords Are Equivalent`` () = 
    let testData = generateTestData 100
    let hashedData = testData |> List.map (fun password -> BCrypt.Net.BCrypt.HashPassword(password, workFactor=4))

    let compareData = List.zip testData hashedData

    compareData 
        |> List.iter (fun (password,hashedPassword) -> Assert.Equal(true, PasswordGenerator.CheckPassword(password, hashedPassword)))


[<Fact>]
let ``Different hashing attempts produce unique results`` () =
    let randomStrings = generateTestData 5 // Generate some test passwords

    // Hash each password multiple times
    let hashedPasswords =
        randomStrings
        |> List.collect (fun password -> 
            [1 .. 10] // Hash each password 10 times
            |> List.map (fun _ -> BCrypt.Net.BCrypt.HashPassword(password)))

    // Check for duplicates across all hashed passwords
    let uniqueHashes = List.distinct hashedPasswords 
    // Assert that all hashes are unique
    Assert.Equal(List.length hashedPasswords , List.length uniqueHashes)
