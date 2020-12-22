namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
        Offices : OfficesHttpHandler
    }

module HttpHandlers =
    let initialize eventStore =
        {
            Bookings = BookingsHttpHandler.initialize eventStore
            Offices = OfficesHttpHandler.initialize ()
        }
