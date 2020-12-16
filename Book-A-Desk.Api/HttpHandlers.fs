namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
    }

module HttpHandlers =
    let initialize eventStore reservationCommandsFactory =
        {
            Bookings = BookingsHttpHandler.initialize eventStore reservationCommandsFactory
        }
