namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
    }

module HttpHandlers =
    let initialize eventStore =
        {
            Bookings = BookingsHttpHandler.initialize eventStore
        }
