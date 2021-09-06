namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
        Offices : OfficesHttpHandler
    }

module HttpHandlers =
    let initialize eventStore reservationCommandsFactory getOffices sendEmailNotification errorHandler =
        {
            Bookings = BookingsHttpHandler.initialize eventStore reservationCommandsFactory sendEmailNotification errorHandler
            Offices = OfficesHttpHandler.initialize eventStore getOffices
        }
