/*
 * ----------------------------------------------
 * JweException — JWE 自訂例外類別
 * 2026-04-13
 * src/NetJwe.Core/Exceptions/JweException.cs
 * ----------------------------------------------
 */

namespace NetJwe.Core.Exceptions;

/// <summary>
/// JWE 加解密過程中發生錯誤時拋出的例外
/// </summary>
public class JweException : Exception
{
    public JweException(string message) : base(message) { }

    public JweException(string message, Exception innerException)
        : base(message, innerException) { }
}
