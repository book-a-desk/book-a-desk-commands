namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
        Offices : OfficesHttpHandler
    }

module HttpHandlers =
    let initialize eventStore reservationCommandsFactory getOffices =
        {
            Bookings = BookingsHttpHandler.initialize eventStore reservationCommandsFactory
            Offices = OfficesHttpHandler.initialize eventStore getOffices
        }
