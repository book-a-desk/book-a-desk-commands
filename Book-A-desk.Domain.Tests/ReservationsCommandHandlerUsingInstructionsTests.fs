namespace Book_A_desk.Domain.Tests

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Commands.Instructions.ReservationInstructions
open Xunit

#nowarn "40"
module rec ReservationsCommandHandlerUsingInstructionsTests =
    [<Fact>]
    let ``Given a Book a desk Command When Command is invalid, then Events are not appended`` () =
        //Arrange
        let office = Offices.All.[0]
        let bookADeskFails =
            {
                // Fails because of the empty email address
                BookADesk.EmailAddress = EmailAddress ""
                BookADesk.Date = DateTime.Now.AddDays(1.)
                BookADesk.OfficeId = office.Id
            }
            |> BookADesk
        
        let mutable eventsWereAppended = false
        let appendEvents _ = eventsWereAppended <- true
        let offices = Offices.All
        let getReservationAggregate _ = 
            {
                Id = ReservationAggregate.Id
                BookedDesks = []
            }
            
        let testInterpreter = reservationTestInterpreter appendEvents getReservationAggregate offices
        
        // Act
        let program = ReservationsCommandHandlerUsingInstructions.handle bookADeskFails
        // running the program
        testInterpreter program
        
        Assert.False(eventsWereAppended)
        
    [<Fact>]
    let ``Given a Book a desk Command When Command is valid, then Events are appended`` () =
        //Arrange
        let office = Offices.All.[0]
        let bookADesk =
            {
                BookADesk.EmailAddress = EmailAddress "something@something.com"
                BookADesk.Date = DateTime.Now.AddDays(1.)
                BookADesk.OfficeId = office.Id
            }
            |> BookADesk
        
        let mutable eventsWereAppended = false
        let appendEvents _ = eventsWereAppended <- true
        let offices = Offices.All
        let getReservationAggregate _ = 
            {
                Id = ReservationAggregate.Id
                BookedDesks = []
            }
            
        let testInterpreter = reservationTestInterpreter appendEvents getReservationAggregate offices
        
        // Act
        let program = ReservationsCommandHandlerUsingInstructions.handle bookADesk
        // running the program
        testInterpreter program
        
        Assert.True(eventsWereAppended)
    
    let rec reservationTestInterpreter appendEvents getReservationAggregate offices program =
        let recurse = reservationTestInterpreter appendEvents getReservationAggregate offices
        
        match program with
        | Stop _ -> ()
        | Next (AppendEvents (events, next)) ->
            appendEvents events
            recurse next
        | Next (GetReservationAggregate (reservationId, next)) ->
            let reservationAggregate = getReservationAggregate reservationId
            recurse (next reservationAggregate)
        | Next (GetOffices next) ->
            recurse (next offices)

