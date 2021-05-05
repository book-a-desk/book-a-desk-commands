namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands

type ReservationCommandsFactory =
    {
        CreateBookADeskCommand: unit -> BookADeskReservationCommand
        //CreateCancelADeskCommand: unit -> CancelADeskReservationCommand
    }

module ReservationCommandsFactory =
    let provide getOffices =

        let createBookADeskCommand () = BookADeskReservationCommand.provide getOffices

        {
            CreateBookADeskCommand = createBookADeskCommand
            //CreateCancelADeskCommand = createCreateCancelADeskCommand
        }
