module Tests

open System
open System.Data
open System.IO
open Xunit
open PostgresDB // Opening DB
open Utilities // Opening Configuration


type TestableChirpyDatabase(dbUrl: string, DevMode: bool) =
    inherit ChirpyDatabase(dbUrl, DevMode)
    member this.ConnectionState = base.ConnectionState
    
   
[<Fact>]
let ``Config Finds .env`` () =
    let cfg = new ConfigurationAccess()
    Assert.Equal(cfg.DevMode, true)

[<Fact>]
let ``Test Database Connection`` () =
    let cfg = new ConfigurationAccess()
    let db = new TestableChirpyDatabase(cfg.DbUrl, cfg.DevMode)
    let openDB = db.OpenAsync()
    openDB.Wait()
    Assert.Equal(ConnectionState.Open, db.ConnectionState)
