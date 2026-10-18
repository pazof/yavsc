using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PostIt.ViewModels;

public partial class AdministrationFieldViewModel : ObservableObject
{
    private readonly Type _type;
    private readonly bool _wasNull;
    public string Name { get; }
    public bool IsBoolean => _type == typeof(bool);
    public bool IsChoice => _type.IsEnum;
    public bool IsText => !IsBoolean && !IsChoice;
    public bool IsMultiline => Name is "Body" or "Description";
    public string[] Choices => IsChoice ? Enum.GetNames(_type) : [];

    [ObservableProperty] public partial string Text { get; set; } = "";
    [ObservableProperty] public partial bool Checked { get; set; }

    private AdministrationFieldViewModel(string name, Type type, object? value)
    {
        Name = name;
        _type = Nullable.GetUnderlyingType(type) ?? type;
        _wasNull = value is null;
        Checked = value is true;
        Text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
    }

    public static IReadOnlyList<AdministrationFieldViewModel> From<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T record) where T : class
        => typeof(T).GetProperties().Select(property =>
            new AdministrationFieldViewModel(property.Name, property.PropertyType, property.GetValue(record))).ToArray();

    public static T Build<T>(IEnumerable<AdministrationFieldViewModel> fields) where T : class
    {
        var values = fields.ToDictionary(field => field.Name, field => field.ReadValue());
        var record = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(values))
            ?? throw new InvalidOperationException("Le formulaire est vide.");
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(record, new ValidationContext(record), errors, true))
            throw new ValidationException(string.Join("; ", errors.Select(error => error.ErrorMessage)));
        return record;
    }

    private object? ReadValue()
    {
        if (IsBoolean) return Checked;
        if (_type == typeof(string)) return _wasNull && Text.Length == 0 ? null : Text;
        if (IsChoice)
        {
            if (!Choices.Contains(Text, StringComparer.Ordinal))
                throw new ValidationException($"{Name} : choisissez une valeur valide.");
            return Text;
        }
        if (_type == typeof(int))
        {
            if (string.IsNullOrWhiteSpace(Text)) return null;
            if (int.TryParse(Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) return number;
            throw new ValidationException($"{Name} : un entier est requis.");
        }
        throw new InvalidOperationException($"Type de champ non pris en charge : {Name}.");
    }
}
