namespace Book_A_Desk.Api

type ApiDependencyFactory =
    {
        CreateBookingsHttpHandler: unit -> BookingsHttpHandler
        CreateOfficesHttpHandler: unit -> OfficesHttpHandler
        CreateNotifierHttpHandler: unit -> NotifierHttpHandler
        CreateFeatureFlagsHttpHandler: unit -> FlagsHttpHandler
    }

module ApiDependencyFactory =
    let provide
        getEventStore
        reservationCommandsFactory
        getOffices
        notifySuccess
        notifyOfficeRestrictions
        getFeatureFlags
        =

        let createBookingsHttpHandler bearerToken =
            BookingsHttpHandler.initialize
                getEventStore
                reservationCommandsFactory
                notifySuccess
                <| BookADeskErrorHandler.initialize()                

        let createOfficesHttpHandler bearerToken =
            OfficesHttpHandler.initialize
                getEventStore
                getOffices

        let createNotifierHttpHandler bearerToken =
            NotifierHttpHandler.initialize
                getEventStore
                notifyOfficeRestrictions
                <| BookADeskErrorHandler.initialize()
                
        let createFeatureFlagsHandler bearerToken =
            FlagsHttpHandler.initialize
                getFeatureFlags

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
            CreateNotifierHttpHandler = createNotifierHttpHandler
            CreateFeatureFlagsHttpHandler = createFeatureFlagsHandler
        }
