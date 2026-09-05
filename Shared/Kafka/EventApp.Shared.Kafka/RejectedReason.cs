namespace EventApp.Shared.Kafka
{
    public enum RejectedReason
    {
        NotFoundEventtException = 0,
        NotFoundUserException = 1,
        PastEventBookingException = 2,
        ActiveLeasesExceededException = 3,
        PermissionDeniedException = 4,
        NoAvailableSeatsException = 5
    }
}
