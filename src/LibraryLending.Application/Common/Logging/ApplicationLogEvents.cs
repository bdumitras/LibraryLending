using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Common.Logging;

public static class ApplicationLogEvents
{
    public static readonly EventId BookCreated = new(1000, nameof(BookCreated));
    public static readonly EventId BookUpdated = new(1001, nameof(BookUpdated));
    public static readonly EventId BookDeleted = new(1002, nameof(BookDeleted));
    public static readonly EventId BooksListed = new(1003, nameof(BooksListed));
    public static readonly EventId BookRetrieved = new(1004, nameof(BookRetrieved));

    public static readonly EventId MemberCreated = new(2000, nameof(MemberCreated));
    public static readonly EventId MemberUpdated = new(2001, nameof(MemberUpdated));
    public static readonly EventId MemberDeleted = new(2002, nameof(MemberDeleted));
    public static readonly EventId MembersListed = new(2003, nameof(MembersListed));
    public static readonly EventId MemberRetrieved = new(2004, nameof(MemberRetrieved));

    public static readonly EventId LoanCreated = new(3000, nameof(LoanCreated));
    public static readonly EventId LoanReturned = new(3001, nameof(LoanReturned));
    public static readonly EventId LoansListed = new(3002, nameof(LoansListed));
    public static readonly EventId LoanRetrieved = new(3003, nameof(LoanRetrieved));
    public static readonly EventId LoanStatusesRefreshed = new(3004, nameof(LoanStatusesRefreshed));
}
