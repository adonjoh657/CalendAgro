using System.Globalization;
using CalendAgro.Models;

namespace CalendAgro.Controls;

// Convertit une phase culturale (TypeEtape) en type de pastille (StepKind).
// Les modèles ne référencent pas les contrôles : la correspondance vit ici.
public sealed class TypeEtapeToStepKindConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TypeEtape phase
            ? phase switch
            {
                TypeEtape.Entretien => StepKind.Entretien,
                TypeEtape.Recolte => StepKind.Recolte,
                _ => StepKind.Semis,
            }
            : StepKind.Semis;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
