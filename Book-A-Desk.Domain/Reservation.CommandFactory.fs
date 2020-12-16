namespace Book_A_Desk.Domain.Reservation

namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain.Reservation.Commands

type ReservationCommandsFactory =
    {
        CreateBookADeskCommand: unit -> BookADeskReservationCommand
    }

module ReservationCommandsFactory =
    let provide () =
        let getValidationResultOf = fun f -> f()

        let createBookADeskCommand () = BookADeskReservationCommand.provide getValidationResultOf

        {
            CreateBookADeskCommand = createBookADeskCommand
        }
