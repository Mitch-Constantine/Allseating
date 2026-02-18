namespace Allseating.Application;

/// <summary>
/// Thrown by infrastructure when a unique constraint on Barcode is violated (e.g. race condition on create/update).
/// </summary>
public sealed class DuplicateBarcodeException : Exception
{
    public DuplicateBarcodeException() : base("A game with this Barcode already exists.") { }
}
