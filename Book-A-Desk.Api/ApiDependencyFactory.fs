namespace Book_A_Desk.Api

type ApiDependencyFactory =
    {
        CreateBookingsHttpHandler: unit -> BookingsHttpHandler
        CreateOfficesHttpHandler: unit -> OfficesHttpHandler
    }

module ApiDependencyFactory =
    let provide
        getEventStore
        reservationCommandsFactory
        getOffices
        sendEmailNotification
        =

        let createBookingsHttpHandler bearerToken =
            BookingsHttpHandler.initialize
                getEventStore
                reservationCommandsFactory
                sendEmailNotification

        let createOfficesHttpHandler bearerToken =
            OfficesHttpHandler.initialize
                getEventStore
                getOffices

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
        }
