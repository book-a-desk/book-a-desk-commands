namespace Book_A_Desk.Api

type ApiDependencyFactory =
    {
        CreateBookingsHttpHandler: unit -> BookingsHttpHandler
        CreateOfficesHttpHandler: unit -> OfficesHttpHandler
    }

module ApiDependencyFactory =
    let provide
        eventStore
        reservationCommandsFactory
        getOffices
        sendEmailNotification
        =

        let createBookingsHttpHandler bearerToken =
            BookingsHttpHandler.initialize
                eventStore
                reservationCommandsFactory
                sendEmailNotification

        let createOfficesHttpHandler bearerToken =
            OfficesHttpHandler.initialize
                eventStore
                getOffices

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
        }
