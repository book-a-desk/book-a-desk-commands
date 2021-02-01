namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
        Offices : OfficesHttpHandler
    }

module HttpHandlers =
    let initialize eventStore getOffices =
        {
            Bookings = BookingsHttpHandler.initialize eventStore getOffices
            Offices = OfficesHttpHandler.initialize eventStore getOffices
        }
