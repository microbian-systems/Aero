namespace Aero.Core.Validation;

internal interface IFieldValidator
{
        /// <summary>
    /// GetErrors method.
    /// </summary>
IEnumerable<ValidationError> GetErrors();
}