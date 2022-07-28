namespace Book_A_Desk.Api

type HttpHandlers =
    {
        Bookings: BookingsHttpHandler
        Offices : OfficesHttpHandler
        CancelBookings : CancelBookingsHttpHandler
    }

module HttpHandlers =
    let initialize eventStore reservationCommandsFactory getOffices sendEmailNotification featureFlags errorHandler =
        {
            Bookings = BookingsHttpHandler.initialize eventStore reservationCommandsFactory sendEmailNotification featureFlags errorHandler
            Offices = OfficesHttpHandler.initialize eventStore getOffices
            CancelBookings = CancelBookingsHttpHandler.initialize eventStore reservationCommandsFactory errorHandler
        }
