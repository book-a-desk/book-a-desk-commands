namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain.Reservation.Commands

type ReservationCommandsFactory =
    {
        CreateBookADeskCommand: unit -> BookADeskReservationCommand
        //CreateCancelADeskCommand: unit -> CancelADeskReservationCommand
    }

module ReservationCommandsFactory =
    let provide getOffices domainName =

        let createBookADeskCommand () = BookADeskReservationCommand.provide (getOffices ()) domainName

        {
            CreateBookADeskCommand = createBookADeskCommand
            //CreateCancelADeskCommand = createCreateCancelADeskCommand
        }
