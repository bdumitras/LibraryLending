using FluentValidation;
using LibraryLending.Application.Common.Contracts;
using LibraryLending.Application.Common.Exceptions;
using LibraryLending.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace LibraryLending.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const int ClientClosedRequestStatusCode = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, response) = MapException(httpContext, exception);

        if (exception is ValidationException validationException)
        {
            var validationErrors = validationException.Errors
                .Select(error => new
                {
                    error.PropertyName,
                    error.ErrorMessage,
                    error.ErrorCode,
                    error.AttemptedValue
                })
                .ToArray();

            logger.LogWarning(
                validationException,
                "Validation failed for {Method} {Path} with {ErrorCount} errors. Errors: {@ValidationErrors}",
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                validationErrors.Length,
                validationErrors);
        }
        else if (exception is DomainValidationException domainValidationException)
        {
            logger.LogWarning(
                domainValidationException,
                "Domain validation failed for {Method} {Path}. ErrorCode {ErrorCode}. Detail: {Detail}",
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                domainValidationException.GetType().Name,
                domainValidationException.Message);
        }
        else if (exception is NotFoundException or ConflictException or DomainException or ApplicationLayerException)
        {
            logger.LogWarning(
                exception,
                "Handled application exception {ExceptionType} for {Method} {Path}. ResponseStatusCode {StatusCode}. Detail: {Detail}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                statusCode,
                exception.Message);
        }
        else if (exception is OperationCanceledException)
        {
            logger.LogInformation(
                "Request was cancelled for {Method} {Path}. ResponseStatusCode {StatusCode}.",
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                statusCode);
        }
        else
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. ResponseStatusCode {StatusCode}.",
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                statusCode);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static (int StatusCode, ErrorResponse Response) MapException(HttpContext httpContext, Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                (StatusCodes.Status400BadRequest, CreateValidationResponse(httpContext, validationException)),

            NotFoundException notFoundException =>
                (StatusCodes.Status404NotFound, CreateErrorResponse(
                    httpContext,
                    title: "Resource not found",
                    status: StatusCodes.Status404NotFound,
                    detail: notFoundException.Message,
                    errorCode: notFoundException.GetType().Name)),

            ConflictException conflictException =>
                (StatusCodes.Status409Conflict, CreateErrorResponse(
                    httpContext,
                    title: "Conflict",
                    status: StatusCodes.Status409Conflict,
                    detail: conflictException.Message,
                    errorCode: conflictException.GetType().Name)),

            DomainValidationException domainValidationException =>
                (StatusCodes.Status400BadRequest, CreateDomainValidationResponse(httpContext, domainValidationException)),

            BookHasNoAvailableCopiesException bookHasNoAvailableCopiesException =>
                (StatusCodes.Status409Conflict, CreateErrorResponse(
                    httpContext,
                    title: "Book unavailable",
                    status: StatusCodes.Status409Conflict,
                    detail: bookHasNoAvailableCopiesException.Message,
                    errorCode: bookHasNoAvailableCopiesException.GetType().Name)),

            BookInventoryOverflowException bookInventoryOverflowException =>
                (StatusCodes.Status409Conflict, CreateErrorResponse(
                    httpContext,
                    title: "Inventory conflict",
                    status: StatusCodes.Status409Conflict,
                    detail: bookInventoryOverflowException.Message,
                    errorCode: bookInventoryOverflowException.GetType().Name)),

            InactiveMemberCannotBorrowException inactiveMemberCannotBorrowException =>
                (StatusCodes.Status409Conflict, CreateErrorResponse(
                    httpContext,
                    title: "Member is not allowed to borrow",
                    status: StatusCodes.Status409Conflict,
                    detail: inactiveMemberCannotBorrowException.Message,
                    errorCode: inactiveMemberCannotBorrowException.GetType().Name)),

            LoanAlreadyReturnedException loanAlreadyReturnedException =>
                (StatusCodes.Status409Conflict, CreateErrorResponse(
                    httpContext,
                    title: "Loan already returned",
                    status: StatusCodes.Status409Conflict,
                    detail: loanAlreadyReturnedException.Message,
                    errorCode: loanAlreadyReturnedException.GetType().Name)),

            InvalidLoanDateRangeException invalidLoanDateRangeException =>
                (StatusCodes.Status400BadRequest, CreateErrorResponse(
                    httpContext,
                    title: "Invalid loan dates",
                    status: StatusCodes.Status400BadRequest,
                    detail: invalidLoanDateRangeException.Message,
                    errorCode: invalidLoanDateRangeException.GetType().Name,
                    errors: CreateSingleErrorDictionary("dueDateUtc", invalidLoanDateRangeException.Message))),

            InvalidLoanReturnDateException invalidLoanReturnDateException =>
                (StatusCodes.Status400BadRequest, CreateErrorResponse(
                    httpContext,
                    title: "Invalid return date",
                    status: StatusCodes.Status400BadRequest,
                    detail: invalidLoanReturnDateException.Message,
                    errorCode: invalidLoanReturnDateException.GetType().Name,
                    errors: CreateSingleErrorDictionary("returnedAtUtc", invalidLoanReturnDateException.Message))),

            DomainException domainException =>
                (StatusCodes.Status409Conflict, CreateErrorResponse(
                    httpContext,
                    title: "Domain rule violated",
                    status: StatusCodes.Status409Conflict,
                    detail: domainException.Message,
                    errorCode: domainException.GetType().Name)),

            ApplicationLayerException applicationLayerException =>
                (StatusCodes.Status400BadRequest, CreateErrorResponse(
                    httpContext,
                    title: "Application request failed",
                    status: StatusCodes.Status400BadRequest,
                    detail: applicationLayerException.Message,
                    errorCode: applicationLayerException.GetType().Name)),

            OperationCanceledException =>
                (ClientClosedRequestStatusCode, CreateErrorResponse(
                    httpContext,
                    title: "Request cancelled",
                    status: ClientClosedRequestStatusCode,
                    detail: "The request was cancelled before completion.",
                    errorCode: nameof(OperationCanceledException))),

            _ =>
                (StatusCodes.Status500InternalServerError, CreateErrorResponse(
                    httpContext,
                    title: "An unexpected error occurred",
                    status: StatusCodes.Status500InternalServerError,
                    detail: "The server encountered an unexpected error.",
                    errorCode: "UnhandledException"))
        };
    }

    private static ErrorResponse CreateValidationResponse(HttpContext httpContext, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(
                failure => string.IsNullOrWhiteSpace(failure.PropertyName) ? "$" : failure.PropertyName,
                failure => failure.ErrorMessage)
            .ToDictionary(
                group => group.Key,
                group => group.Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        return CreateErrorResponse(
            httpContext,
            title: "Validation failed",
            status: StatusCodes.Status400BadRequest,
            detail: "One or more validation errors occurred.",
            errorCode: nameof(ValidationException),
            errors: errors);
    }

    private static ErrorResponse CreateDomainValidationResponse(HttpContext httpContext, DomainValidationException exception)
    {
        return CreateErrorResponse(
            httpContext,
            title: "Domain validation failed",
            status: StatusCodes.Status400BadRequest,
            detail: exception.Message,
            errorCode: exception.GetType().Name,
            errors: CreateSingleErrorDictionary("domain", exception.Message));
    }

    private static ErrorResponse CreateErrorResponse(
        HttpContext httpContext,
        string title,
        int status,
        string detail,
        string errorCode,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var path = httpContext.Request.Path.HasValue
            ? httpContext.Request.Path.Value
            : httpContext.Request.Path.ToString();

        return new ErrorResponse
        {
            Type = $"https://httpstatuses.com/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = path,
            ErrorCode = errorCode,
            Errors = errors
        };
    }

    private static IReadOnlyDictionary<string, string[]> CreateSingleErrorDictionary(string key, string message)
    {
        return new Dictionary<string, string[]>
        {
            [key] = [message]
        };
    }
}
