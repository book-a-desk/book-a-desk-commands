namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
        Offices : OfficesHttpHandler
    }

module HttpHandlers =
    let initialize eventStore reservationCommandsFactory getOffices sendEmailNotification =
        {
            Bookings = BookingsHttpHandler.initialize eventStore reservationCommandsFactory sendEmailNotification
            Offices = OfficesHttpHandler.initialize eventStore getOffices
        }
