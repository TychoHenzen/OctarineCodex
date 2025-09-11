using System;
using System.Runtime.CompilerServices;
using OctarineCodex.Services;

namespace OctarineCodex.Logging;

/// <summary>
///     Provides logging functionality with automatic caller information capture.
///     Supports multiple log levels and file-based output with rotation.
/// </summary>
[Service<LoggingService>]
public interface ILoggingService
{
    /// <summary>
    ///     Logs a debug message with caller information.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="memberName">Automatically captured caller member name.</param>
    /// <param name="sourceFilePath">Automatically captured caller file path.</param>
    /// <param name="sourceLineNumber">Automatically captured caller line number.</param>
    void Debug(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    ///     Logs an informational message with caller information.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="memberName">Automatically captured caller member name.</param>
    /// <param name="sourceFilePath">Automatically captured caller file path.</param>
    /// <param name="sourceLineNumber">Automatically captured caller line number.</param>
    void Info(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    ///     Logs a warning message with caller information.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="memberName">Automatically captured caller member name.</param>
    /// <param name="sourceFilePath">Automatically captured caller file path.</param>
    /// <param name="sourceLineNumber">Automatically captured caller line number.</param>
    void Warn(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    ///     Logs an error message with caller information.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="memberName">Automatically captured caller member name.</param>
    /// <param name="sourceFilePath">Automatically captured caller file path.</param>
    /// <param name="sourceLineNumber">Automatically captured caller line number.</param>
    void Error(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    ///     Logs an exception with caller information.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Optional additional message.</param>
    /// <param name="memberName">Automatically captured caller member name.</param>
    /// <param name="sourceFilePath">Automatically captured caller file path.</param>
    /// <param name="sourceLineNumber">Automatically captured caller line number.</param>
    void Exception(Exception exception, string? message = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);
}