namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Cancellation.Commands

type ReservationCommandsFactory =
    {
        CreateBookADeskCommand: unit -> BookADeskReservationCommand
        CreateCancelBookADeskCommand: unit -> BookADeskCancellationCommand
    }

module ReservationCommandsFactory =
    let provide getOffices domainName =

        let createBookADeskCommand () = BookADeskReservationCommand.provide (getOffices ()) domainName
        let createCancelBookADeskCommand () = BookADeskCancellationCommand.provide (getOffices ()) domainName

        {
            CreateBookADeskCommand = createBookADeskCommand
            CreateCancelBookADeskCommand = createCancelBookADeskCommand
        }
